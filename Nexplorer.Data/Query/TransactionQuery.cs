using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Data.Context;
using Nexplorer.Domain.Criteria;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Data.Query
{
    public class TransactionQuery
    {
        private readonly NexusDb _nexusDb;
        private readonly IMapper _mapper;
        private readonly RedisCommand _redisCommand;
        private readonly BlockCacheService _cache;
        
        public TransactionQuery(NexusDb nexusDb, IMapper mapper, RedisCommand redisCommand, BlockCacheService cache)
        {
            _nexusDb = nexusDb;
            _mapper = mapper;
            _redisCommand = redisCommand;
            _cache = cache;
        }

        public async Task<TransactionDto> GetTransaction(string txHash)
        {
            var cacheTx = await _cache.GetTransactionAsync(txHash);

            if (cacheTx != null)
                return cacheTx;
            
            var tx = await _nexusDb.Transactions
                .Where(x => x.Hash == txHash)
                .Include(x => x.InputOutputs)
                .ThenInclude(x => x.Address)
                .Include(x => x.Block)
                .FirstOrDefaultAsync();

            return _mapper.Map<TransactionDto>(tx);
        }
        
        public async Task<IEnumerable<TransactionLiteDto>> GetNewTransactionCacheAsync()
        {
            return await _redisCommand.GetAsync<IEnumerable<TransactionLiteDto>>(Settings.Redis.TransactionLiteCache);
        }

        public async Task<int> GetTransactionCountLastDay()
        {
            return await _redisCommand.GetAsync<int>(Settings.Redis.TransactionCount24Hours);
        }

        public async Task<FilterResult<TransactionDto>> GetTransactionsFilteredAsync(TransactionInputOutputType? txIoType, TransactionFilterCriteria filter, int start, int count, bool countResults, int? maxResults = null)
        {
            const string from = @"
                FROM [dbo].[Transaction] t
                INNER JOIN [dbo].[TransactionInputOutput] tInOut ON tInOut.[TransactionId] = t.[TransactionId] 
                INNER JOIN [dbo].[Address] a ON a.[AddressId] = tInOut.[AddressId] 
                WHERE 1 = 1 ";
            
            var where = BuildWhereClause(txIoType, filter, out var param);

            var sqlOrderBy = "ORDER BY ";

            switch (filter.OrderBy)
            {
                case OrderTransactionsBy.LowestAmount:
                    sqlOrderBy += "InputOutputAmount ";
                    break;
                case OrderTransactionsBy.HighestAmount:
                    sqlOrderBy += "InputOutputAmount DESC ";
                    break;
                case OrderTransactionsBy.LeastRecent:
                    sqlOrderBy += "t.[Timestamp] ";
                    break;
                case OrderTransactionsBy.MostRecent:
                    sqlOrderBy += "t.[Timestamp] DESC ";
                    break;
                default:
                    sqlOrderBy += "InputOutputAmount DESC ";
                    break;
            }

            var sqlQ = $@"SELECT
                          t.[TransactionId],
                          t.[Hash] AS TransactionHash,
                          t.[BlockHeight],
                          t.[Timestamp],
                          t.[Amount] AS Total,
                          t.[TransactionType],
                          tInOut.[TransactionInputOutputType],
                          tInOut.[Amount] AS InputOutputAmount,
                          a.[Hash] AS AddressHash
                          {from}
                          {where}                                          
                          {sqlOrderBy}                           
                          OFFSET @start ROWS FETCH NEXT @count ROWS ONLY;";

            var sqlC = $@"SELECT 
                         COUNT(*)
                         FROM (SELECT TOP (@maxResults)
                               1 AS Cnt
                               {from}
                               {where}) AS resultCount;";

            using (var sqlCon = await DbConnectionFactory.GetNexusDbConnectionAsync())
            {
                var results = new FilterResult<TransactionDto>();

                var cacheTxs = FilterCacheBlocks(await _cache.GetBlocksAsync(), txIoType, filter)
                    .Skip(start)
                    .ToList();

                var cacheCount = cacheTxs.Sum(x => x.Inputs.Concat(x.Outputs).Count());

                param.Add(nameof(count), count);
                param.Add(nameof(start), start);
                param.Add(nameof(maxResults), maxResults ?? int.MaxValue);

                if (cacheCount >= count)
                {
                    results.Results = cacheTxs.Take(count).ToList();

                    results.ResultCount = countResults
                        ? cacheCount + (int) (await sqlCon.QueryAsync<int>(sqlC, param)).FirstOrDefault()
                        : -1;
                }
                else
                {
                    using (var multi = await sqlCon.QueryMultipleAsync(string.Concat(sqlQ, sqlC), param))
                    {
                        results.Results = cacheTxs.Concat(MapTransactions(await multi.ReadAsync())).ToList();
                        results.ResultCount = countResults
                            ? cacheCount + (int) (await multi.ReadAsync<int>()).FirstOrDefault()
                            : -1;
                    }
                }

                switch (filter.OrderBy)
                {
                    case OrderTransactionsBy.MostRecent:
                        results.Results = results.Results.OrderByDescending(x => x.Timestamp).ToList();
                        break;
                    case OrderTransactionsBy.LeastRecent:
                        results.Results = results.Results.OrderBy(x => x.Timestamp).ToList();
                        break;
                    case OrderTransactionsBy.HighestAmount:
                        results.Results = results.Results.OrderByDescending(x => x.Inputs.Concat(x.Outputs).Max(y => y.Amount)).ToList();
                        break;
                    case OrderTransactionsBy.LowestAmount:
                        results.Results = results.Results.OrderBy(x => x.Inputs.Concat(x.Outputs).Max(y => y.Amount)).ToList();
                        break;
                }

                var txs = new List<TransactionDto>();
                var ioCount = 0;

                foreach (var r in results.Results)
                {
                    txs.Add(r);

                    ioCount += r.Inputs.Count + r.Outputs.Count;

                    if (ioCount >= count)
                        break;
                }


                results.Results = txs;

                return results;
            }
        }

        public async Task<Dictionary<string, List<TransactionAddressDto>>> GetTransactionAddresses(List<TransactionDto> transactions)
        {
            var txAdds = new Dictionary<string, List<TransactionAddressDto>>();

            foreach (var txDto in transactions)
            {
                var tx = txDto.TransactionId == 0 
                    ? await _cache.GetTransactionAsync(txDto.Hash)
                    : await _nexusDb.Transactions
                        .Include(x => x.InputOutputs)
                        .ThenInclude(x => x.Address)
                        .Where(x => x.TransactionId == txDto.TransactionId)
                        .Select(x => _mapper.Map<Transaction, TransactionDto>(x))
                        .FirstOrDefaultAsync();

                if (tx == null)
                    throw new NullReferenceException();

                var txIos = tx.Inputs.Concat(tx.Outputs).Select(x => new TransactionAddressDto
                {
                    AddressHash = x.AddressHash,
                    TransactionInputOutputType = x.TransactionInputOutputType
                }).ToList();

                if (txAdds.ContainsKey(tx.Hash))
                    txAdds[tx.Hash].AddRange(txIos);
                else
                    txAdds.Add(tx.Hash, txIos);
            }

            return txAdds;
        }

        private static List<TransactionDto> MapTransactions(IEnumerable<dynamic> dataset)
        {
            var txs = new Dictionary<int, TransactionDto>();

            foreach (var rawTx in dataset)
            {
                var exists = txs.ContainsKey(rawTx.TransactionId);

                TransactionDto tx = exists
                    ? txs[rawTx.TransactionId]
                    : new TransactionDto
                    {
                        TransactionId = (int)rawTx.TransactionId,
                        Hash = (string)rawTx.TransactionHash,
                        Amount = (double)rawTx.Total,
                        BlockHeight = (int)rawTx.BlockHeight,
                        Timestamp = (DateTime)rawTx.Timestamp,
                        TransactionType = (TransactionType)rawTx.TransactionType,
                        Inputs = new List<TransactionInputOutputLiteDto>(),
                        Outputs = new List<TransactionInputOutputLiteDto>()
                    };

                var txInOut = new TransactionInputOutputLiteDto
                {
                    AddressHash = (string)rawTx.AddressHash,
                    Amount = (double)rawTx.InputOutputAmount,
                    TransactionInputOutputType = (TransactionInputOutputType)rawTx.TransactionInputOutputType
                };

                switch (txInOut.TransactionInputOutputType)
                {
                    case TransactionInputOutputType.Input:
                        tx.Inputs.Add(txInOut);
                        break;
                    case TransactionInputOutputType.Output:
                        tx.Outputs.Add(txInOut);
                        break;
                }

                if (!exists)
                    txs.Add(tx.TransactionId, tx);
            }

            return txs.Values.ToList();
        }

        private bool MatchAddressHash(string[] hashesOne, string[] hashesTwo)
        {
            if (hashesOne == null || !hashesOne.Any() || hashesTwo == null || !hashesTwo.Any())
                return false;

            return hashesOne.Any(x => hashesTwo.Any(y => y == x));
        }

        private static IEnumerable<TransactionDto> FilterCacheBlocks(IEnumerable<BlockDto> blocks, TransactionInputOutputType? txIoType, TransactionFilterCriteria filter)
        {
            return blocks.SelectMany(x => x.Transactions
                .Where(y => (FilterRewardTypes(filter.IsStakeReward, filter.IsMiningReward, y.TransactionType)) &&
                            (!filter.MinAmount.HasValue || y.Amount >= filter.MinAmount) &&
                            (!filter.MaxAmount.HasValue || y.Amount <= filter.MaxAmount) &&
                            (!filter.HeightFrom.HasValue || y.BlockHeight >= filter.HeightFrom) &&
                            (!filter.HeightTo.HasValue || y.BlockHeight <= filter.HeightTo) &&
                            (!filter.UtcFrom.HasValue || y.Timestamp >= filter.UtcFrom) &&
                            (!filter.UtcTo.HasValue || y.Timestamp <= filter.UtcTo))
                .Select(y => new TransactionDto
                {
                    TransactionId = y.TransactionId,
                    Amount = y.Amount,
                    BlockHeight = y.BlockHeight,
                    Confirmations = y.Confirmations,
                    Hash = y.Hash,
                    Timestamp = y.Timestamp,
                    TransactionType = y.TransactionType,
                    Inputs = !txIoType.HasValue || txIoType == TransactionInputOutputType.Input
                        ? y.Inputs.Where(z =>
                            filter.AddressHashes == null || !filter.AddressHashes.Any() ||
                            filter.AddressHashes.Any(hash => hash == z.AddressHash)).ToList()
                        : new List<TransactionInputOutputLiteDto>(),
                    Outputs = !txIoType.HasValue || txIoType == TransactionInputOutputType.Output
                        ? y.Outputs.Where(z =>
                            filter.AddressHashes == null || !filter.AddressHashes.Any() ||
                            filter.AddressHashes.Any(hash => hash == z.AddressHash)).ToList()
                        : new List<TransactionInputOutputLiteDto>(),
                })
                .Where(y => y.Inputs.Concat(y.Outputs).Any()));
        }

        private static bool FilterRewardTypes(bool? isStake, bool? isMine, TransactionType tt)
        {
            if (!isMine.HasValue && !isStake.HasValue)
                return true;

            if (isMine.HasValue && isStake.HasValue)
            {
                if (isMine.Value && isStake.Value)
                    return tt == TransactionType.CoinbaseHash || tt == TransactionType.CoinbasePrime || tt == TransactionType.Coinstake;
                else if (!isMine.Value && !isStake.Value)
                    return tt != TransactionType.CoinbaseHash && tt != TransactionType.CoinbasePrime && tt != TransactionType.Coinstake;
            }
            else if (isMine.HasValue)
            {
                return isMine.Value
                    ? tt == TransactionType.CoinbaseHash || tt == TransactionType.CoinbasePrime
                    : tt != TransactionType.CoinbaseHash && tt != TransactionType.CoinbasePrime;
            }
            else
            {
                return isStake.Value
                    ? tt == TransactionType.Coinstake
                    : tt != TransactionType.Coinstake;
            }

            return true;
        }

        private static string BuildWhereClause(TransactionInputOutputType? txIoType, TransactionFilterCriteria filter, out DynamicParameters param)
        {
            param = new DynamicParameters();

            param.Add(nameof(txIoType), txIoType);

            var whereClause = new StringBuilder();

            if (txIoType.HasValue)
                whereClause.Append($"AND tInOut.[TransactionInputOutputType] = @txIoType ");

            if (filter.MinAmount.HasValue)
            {
                var min = filter.MinAmount.Value;
                param.Add(nameof(min), min);
                whereClause.Append($"AND t.[Amount] >= @min ");
            }

            if (filter.MaxAmount.HasValue)
            {
                var max = filter.MaxAmount.Value;
                param.Add(nameof(max), max);
                whereClause.Append($"AND t.[Amount] <= @max ");
            }

            if (filter.HeightFrom.HasValue)
            {
                var fromHeight = filter.HeightFrom.Value;
                param.Add(nameof(fromHeight), fromHeight);
                whereClause.Append($"AND t.[BlockHeight] <= @fromHeight ");
            }

            if (filter.HeightTo.HasValue)
            {
                var toHeight = filter.HeightTo.Value;
                param.Add(nameof(toHeight), toHeight);
                whereClause.Append($"AND t.[BlockHeight] >= @toHeight ");
            }

            if (filter.UtcFrom.HasValue)
            {
                var fromDate = filter.UtcFrom.Value;
                param.Add(nameof(fromDate), fromDate);
                whereClause.Append($"AND t.[Timestamp] <= @fromDate ");
            }

            if (filter.UtcTo.HasValue)
            {
                var toDate = filter.UtcTo.Value;
                param.Add(nameof(toDate), toDate);
                whereClause.Append($"AND t.[Timestamp] >= @toDate ");
            }
            
            if (filter.IsMiningReward.HasValue || filter.IsStakeReward.HasValue)
            {
                if (filter.IsMiningReward.HasValue && filter.IsStakeReward.HasValue)
                {
                    if (filter.IsMiningReward.Value && filter.IsStakeReward.Value)
                        whereClause.Append($"AND (t.[TransactionType] = {(int)TransactionType.CoinbaseHash} OR t.[TransactionType] = {(int)TransactionType.CoinbasePrime} OR t.[TransactionType] = {(int)TransactionType.Coinstake}) ");
                    else if (!filter.IsMiningReward.Value && !filter.IsStakeReward.Value)
                        whereClause.Append($"AND (t.[TransactionType] <> {(int)TransactionType.CoinbaseHash} AND t.[TransactionType] <> {(int)TransactionType.CoinbasePrime} AND t.[TransactionType] <> {(int)TransactionType.Coinstake}) ");
                }
                else if (filter.IsMiningReward.HasValue)
                {
                    whereClause.Append(filter.IsMiningReward.Value
                        ? $"AND (t.[TransactionType] = {(int) TransactionType.CoinbaseHash} OR t.[TransactionType] = {(int) TransactionType.CoinbasePrime}) "
                        : $"AND (t.[TransactionType] <> {(int) TransactionType.CoinbaseHash} AND t.[TransactionType] <> {(int) TransactionType.CoinbasePrime}) ");
                }
                else
                {
                    whereClause.Append(filter.IsStakeReward.Value
                        ? $"AND t.[TransactionType] = {(int) TransactionType.Coinstake} "
                        : $"AND t.[TransactionType] <> {(int) TransactionType.Coinstake} ");
                }
            }

            if (filter.AddressHashes != null && filter.AddressHashes.Any())
            {
                var addressHashes = filter.AddressHashes;
                param.Add(nameof(addressHashes), addressHashes);
                whereClause.Append("AND a.[Hash] IN @addressHashes ");
            }

            return whereClause.ToString();
        }
    }
}