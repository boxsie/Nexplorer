using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Context;
using Nexplorer.Data.Services;
using Nexplorer.Domain.Criteria;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Data.Query
{
    public class BlockQuery
    {
        private readonly RedisCommand _redisCommand;
        private readonly NexusDb _nexusDb;
        private readonly IMapper _mapper;

        public BlockQuery(RedisCommand redisCommand, NexusDb nexusDb, IMapper mapper)
        {
            _redisCommand = redisCommand;
            _nexusDb = nexusDb;
            _mapper = mapper;
        }

        public async Task<int> GetLastHeightAsync()
        {
            const string sqlQ = @"SELECT MAX(b.[Height]) FROM [dbo].[Block] b";

            using (var connection = new SqlConnection(Settings.Connection.NexusDb))
            {
                connection.Open();

                var height = (await connection.QueryAsync<int>(sqlQ)).FirstOrDefault();

                return height;
            }
        }

        public async Task<int> GetChannelHeightAsync(BlockChannels channel, int height = 0)
        {
            return height == 0
                ? await _nexusDb.Blocks.CountAsync(x => x.Channel == (int)channel)
                : await _nexusDb.Blocks.CountAsync(x => x.Channel == (int)channel && x.Height <= height);
        }

        public async Task<BlockDto> GetBlockAsync(int height, bool includeTransactions)
        {
            return await GetBlockAsync(height, null, includeTransactions);
        }

        public async Task<BlockDto> GetBlockAsync(string hash, bool includeTransactions)
        {
            return await GetBlockAsync(0, hash, includeTransactions);
        }

        private async Task<BlockDto> GetBlockAsync(int height, string hash, bool includeTransactions)
        {
            const string txSqlQ = @"
                INNER JOIN [dbo].[Transaction] t ON t.[BlockHeight] = b.[Height] 
                INNER JOIN [dbo].[TransactionInputOutput] tIo ON tIo.[TransactionId] = t.[TransactionId]
                INNER JOIN [dbo].[Address] a ON a.[AddressId] = tIo.[AddressId] ";

            var sqlQ = $@"
                SELECT 
	              *
                FROM [dbo].[Block] b 
                {(includeTransactions ? txSqlQ : "")}
                WHERE {(hash != null
                ? "b.[Hash] = @hash "
                : "b.[Height] = @height ")}";

            using (var connection = new SqlConnection(Settings.Connection.NexusDb))
            {
                connection.Open();

                Block b = null;

                if (includeTransactions)
                {
                    var results = await connection.QueryAsync<Block, Transaction, TransactionInputOutput, Address, Block>(
                        sqlQ, (bk, tx, txIo, add) =>
                        {
                            if (b == null)
                            {
                                b = bk;
                                b.Transactions = new List<Transaction>();
                            }

                            var t = b.Transactions.FirstOrDefault(x => x.TransactionId == tx.TransactionId);

                            if (t == null)
                            {
                                t = tx;
                                t.Block = b;
                                t.InputOutputs = new List<TransactionInputOutput>();
                                b.Transactions.Add(t);
                            }

                            txIo.Transaction = t;
                            txIo.Address = add;
                            t.InputOutputs.Add(txIo);

                            return b;
                        },
                        splitOn: "TransactionId,TransactionInputOutputId,AddressId",
                        param: new { height, hash });

                    return _mapper.Map<BlockDto>(results.Distinct().FirstOrDefault());
                }
                else
                {
                    var results = await connection.QueryAsync<Block>(sqlQ, new { height, hash });
                    return _mapper.Map<BlockDto>(results.FirstOrDefault());
                }
            }
        }

        public async Task<BlockDto> GetBlockAsync(DateTime time)
        {
            var block = await _nexusDb.Blocks.OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync(x => x.Timestamp < time);

            return _mapper.Map<BlockDto>(block);
        }

        public async Task<FilterResult<BlockLiteDto>> GetBlocksFilteredAsync(BlockFilterCriteria filter, int start, int count, bool countResults, int? maxResults = null)
        {
            const string from = @"
                FROM [dbo].[Block] b
                INNER JOIN [dbo].[Transaction] t ON t.[BlockHeight] = b.[Height]
                WHERE 1 = 1 ";

            const string groupBy = @"
                GROUP BY b.[Height], b.[Hash], b.[Size], b.[Channel], b.[Version], b.[MerkleRoot], 
		                 b.[Timestamp], b.[Nonce], b.[Bits], b.[Difficulty], b.[Mint] ";

            var where = BuildWhereClause(filter, out var param);

            var sqlOrderBy = "ORDER BY ";

            switch (filter.OrderBy)
            {
                case OrderBlocksBy.Highest:
                    sqlOrderBy += "b.[Height] DESC ";
                    break;
                case OrderBlocksBy.Lowest:
                    sqlOrderBy += "b.[Height] ";
                    break;
                case OrderBlocksBy.Largest:
                    sqlOrderBy += "b.[Size] DESC ";
                    break;
                case OrderBlocksBy.Smallest:
                    sqlOrderBy += "b.[Size] ";
                    break;
                default:
                    sqlOrderBy += "b.[Height] DESC ";
                    throw new ArgumentOutOfRangeException();
            }

            var sqlQ = $@"SELECT
                          b.[Height],
	                      b.[Hash],
                          b.[Size],
                          b.[Channel],
                          b.[Timestamp],
                          b.[Difficulty],
                          b.[Mint],
	                      COUNT(t.[TransactionId]) AS TransactionCount
                          {from}
                          {where} 
                          {groupBy}                                          
                          {sqlOrderBy}
                          OFFSET @start ROWS FETCH NEXT @count ROWS ONLY;";

            var sqlC = $@"SELECT 
                         COUNT(*)
                         FROM (SELECT TOP (@maxResults)
                               1 AS Cnt
                               {from}
                               {where}
                               {groupBy}) AS resultCount;";

            using (var sqlCon = await DbConnectionFactory.GetNexusDbConnectionAsync())
            {
                var results = new FilterResult<BlockLiteDto>();
                
                param.Add(nameof(count), count);
                param.Add(nameof(start), start);
                param.Add(nameof(maxResults), maxResults ?? int.MaxValue);

                using (var multi = await sqlCon.QueryMultipleAsync(string.Concat(sqlQ, sqlC), param))
                {
                    var dbBlocks = (await multi.ReadAsync()).Select(x => new BlockLiteDto
                    {
                        Height = x.Height,
                        Hash = x.Hash,
                        Size = x.Size,
                        Channel = ((BlockChannels)x.Channel).ToString(),
                        Timestamp = x.Timestamp,
                        Difficulty = x.Difficulty,
                        Mint = x.Mint,
                        TransactionCount = x.TransactionCount
                    });

                    results.Results = dbBlocks.ToList();
                    results.ResultCount = countResults
                        ? (int)(await multi.ReadAsync<int>()).FirstOrDefault()
                        : -1;
                }

                switch (filter.OrderBy)
                {
                    case OrderBlocksBy.Highest:
                        results.Results = results.Results.OrderByDescending(x => x.Height).ToList();
                        break;
                    case OrderBlocksBy.Lowest:
                        results.Results = results.Results.OrderBy(x => x.Height).ToList();
                        break;
                    case OrderBlocksBy.Largest:
                        results.Results = results.Results.OrderByDescending(x => x.Size).ToList();
                        break;
                    case OrderBlocksBy.Smallest:
                        results.Results = results.Results.OrderBy(x => x.Size).ToList();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                results.Results = results.Results.Take(count).ToList();

                return results;
            }
        }

        public async Task<string> GetBlockHashAsync(int height)
        {
            return await _nexusDb.Blocks.Where(x => x.Height == height)
                .Select(x => x.Hash)
                .FirstOrDefaultAsync();
        }

        public async Task<DateTime> GetBlockTimestamp(int height)
        {
            return height > 0
                ? await _nexusDb.Blocks.Where(x => x.Height == height).Select(x => x.Timestamp).FirstOrDefaultAsync()
                : new DateTime();
        }

        public async Task<BlockDto> GetLastBlockAsync()
        {
            return await GetBlockAsync(await GetLastHeightAsync(), true);
        }

        public async Task<BlockDto> GetLastBlock(BlockChannels channel)
        {
            return await GetBlockAsync(await _nexusDb.Blocks
                .Where(x => x.Channel == (int)channel)
                .MaxAsync(x => x.Height), false);
        }

        public async Task<Transaction> GetLastTransaction()
        {
            return await _nexusDb.Transactions.LastAsync();
        }

        public async Task<int> GetBlockCount(DateTime from, int days)
        {
            return await _nexusDb.Blocks.CountAsync(x => x.Timestamp >= from.AddDays(-days));
        }

        public async Task<int> GetTransactionCount(DateTime from, int days)
        {
            return await _nexusDb.Transactions.CountAsync(x => x.Timestamp >= from.AddDays(-days));
        }

        public async Task LatestBlockSubscribeAsync(Func<BlockLiteDto, Task> onPublish)
        {
            var redisKey = Settings.Redis.NewBlockPubSub;

            await _redisCommand.SubscribeAsync<BlockLiteDto>(redisKey, async value =>
            {
                var block = value;

                await onPublish.Invoke(block);
            });
        }

        public async Task BlockCountLastDayKeySubscribeAsync(Func<int, Task> onPublish)
        {
            var redisKey = Settings.Redis.BlockCount24Hours;

            await _redisCommand.SubscribeAsync<int>(redisKey, async value =>
            {
                var lastDayCount = value;

                await onPublish.Invoke(lastDayCount);
            });
        }

        public async Task<int> GetBlockCountLastDayAsync()
        {
            return await _redisCommand.GetAsync<int>(Settings.Redis.BlockCount24Hours);
        }

        private static IEnumerable<BlockLiteDto> FilterCacheBlocks(IEnumerable<BlockDto> blocks, BlockFilterCriteria filter)
        {
            return blocks.Where(y => 
                            (!filter.HeightFrom.HasValue || y.Height >= filter.HeightFrom) &&
                            (!filter.HeightTo.HasValue || y.Height <= filter.HeightTo) &&
                            (!filter.MinSize.HasValue || y.Size >= filter.MinSize) &&
                            (!filter.MaxSize.HasValue || y.Size <= filter.MaxSize) &&
                            (!filter.UtcFrom.HasValue || y.Timestamp >= filter.UtcFrom) &&
                            (!filter.UtcTo.HasValue || y.Timestamp <= filter.UtcTo))
                .Select(y => new BlockLiteDto(y));
        }

        private static string BuildWhereClause(BlockFilterCriteria filter, out DynamicParameters param)
        {
            param = new DynamicParameters();

            var whereClause = new StringBuilder();
            
            if (filter.HeightFrom.HasValue)
            {
                var fromHeight = filter.HeightFrom.Value;
                param.Add(nameof(fromHeight), fromHeight);
                whereClause.Append($"AND b.[Height] <= @fromHeight ");
            }

            if (filter.HeightTo.HasValue)
            {
                var toHeight = filter.HeightTo.Value;
                param.Add(nameof(toHeight), toHeight);
                whereClause.Append($"AND b.[Height] >= @toHeight ");
            }

            if (filter.MinSize.HasValue)
            {
                var min = filter.MinSize.Value;
                param.Add(nameof(min), min);
                whereClause.Append($"AND b.[Size] >= @min ");
            }

            if (filter.MaxSize.HasValue)
            {
                var max = filter.MaxSize.Value;
                param.Add(nameof(max), max);
                whereClause.Append($"AND b.[Size] <= @max ");
            }

            if (filter.UtcFrom.HasValue)
            {
                var fromDate = filter.UtcFrom.Value;
                param.Add(nameof(fromDate), fromDate);
                whereClause.Append($"AND b.[Timestamp] <= @fromDate ");
            }

            if (filter.UtcTo.HasValue)
            {
                var toDate = filter.UtcTo.Value;
                param.Add(nameof(toDate), toDate);
                whereClause.Append($"AND b.[Timestamp] >= @toDate ");
            }

            return whereClause.ToString();
        }
    }
}
