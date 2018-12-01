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

        public async Task<FilterResult<TransactionDto>> GetTransactionsFilteredAsync(TransactionType txType, TransactionFilterCriteria filter, int start, int count, bool countResults, int? maxResults = null)
        {
            const string from = @"
                FROM [dbo].[Transaction] t
                INNER JOIN [dbo].[TransactionInputOutput] tInOut ON tInOut.[TransactionId] = t.[TransactionId] 
                INNER JOIN [dbo].[Address] a ON a.[AddressId] = tInOut.[AddressId] 
                WHERE 1 = 1 ";
            
            var where = BuildWhereClause(txType, filter, out var param);

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
                          t.[RewardType],
                          tInOut.[TransactionType],
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

                var cacheTxs = FilterCacheBlocks(await _cache.GetBlocksAsync(), txType, filter)
                    .Skip(start)
                    .ToList();

                var cacheCount = cacheTxs.Sum(x => x.Inputs.Concat(x.Outputs).Count());

                param.Add(nameof(count), count);
                param.Add(nameof(start), start);
                param.Add(nameof(maxResults), maxResults ?? int.MaxValue);

                if (countResults)
                {
                    if (cacheCount >= count)
                    {
                        results.Results = cacheTxs.Take(count).ToList();
                        results.ResultCount = cacheCount + (int)(await sqlCon.QueryAsync<int>(sqlC, param)).FirstOrDefault();
                    }
                    else
                    {
                        using (var multi = await sqlCon.QueryMultipleAsync(string.Concat(sqlQ, sqlC), param))
                        {
                            results.Results = cacheTxs.Concat(MapTransactions(await multi.ReadAsync())).ToList();
                            results.ResultCount = cacheCount + (int)(await multi.ReadAsync<int>()).FirstOrDefault();
                        }
                    }
                }
                else
                {
                    results.Results = cacheCount >= count 
                        ? cacheTxs.Take(count).ToList() 
                        : cacheTxs.Concat(MapTransactions(await sqlCon.QueryAsync(sqlQ, param))).ToList();

                    results.ResultCount = -1;
                }

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
                    TransactionType = x.TransactionType
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
                        RewardType = (BlockRewardType)rawTx.RewardType,
                        Inputs = new List<TransactionInputOutputLiteDto>(),
                        Outputs = new List<TransactionInputOutputLiteDto>()
                    };

                var txInOut = new TransactionInputOutputLiteDto
                {
                    AddressHash = (string)rawTx.AddressHash,
                    Amount = (double)rawTx.InputOutputAmount,
                    TransactionType = (TransactionType)rawTx.TransactionType
                };

                switch (txInOut.TransactionType)
                {
                    case TransactionType.Input:
                        tx.Inputs.Add(txInOut);
                        break;
                    case TransactionType.Output:
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

        private static List<TransactionDto> FilterCacheBlocks(IEnumerable<BlockDto> blocks, TransactionType txType, TransactionFilterCriteria filter)
        {
            var txs = blocks.SelectMany(x => x.Transactions
                .Where(y => (!filter.MinAmount.HasValue || y.Amount >= filter.MinAmount) &&
                            (!filter.MaxAmount.HasValue || y.Amount <= filter.MaxAmount) &&
                            (!filter.HeightFrom.HasValue || y.BlockHeight >= filter.HeightFrom) &&
                            (!filter.HeightTo.HasValue || y.BlockHeight <= filter.HeightTo) &&
                            (!filter.UtcFrom.HasValue || y.Timestamp >= filter.UtcFrom) &&
                            (!filter.UtcTo.HasValue || y.Timestamp <= filter.UtcTo) &&
                            (!filter.IsStakeReward.HasValue || y.RewardType == BlockRewardType.Staking) &&
                            (!filter.IsMiningReward.HasValue || y.RewardType == BlockRewardType.Mining))).ToList();

            if (txType != TransactionType.Both)
            {
                foreach (var tx in txs)
                {
                    switch (txType)
                    {
                        case TransactionType.Input:
                            tx.Outputs = new List<TransactionInputOutputLiteDto>();
                            break;
                        case TransactionType.Output:
                            tx.Inputs = new List<TransactionInputOutputLiteDto>();
                            break;
                    }
                }
            }

            if (filter.AddressHashes.Any())
                txs = txs.Where(x => x.Inputs.Concat(x.Outputs).Any(y => filter.AddressHashes.Any(z => z == y.AddressHash))).ToList();

            return txs.ToList();
        }

        private static string BuildWhereClause(TransactionType txType, TransactionFilterCriteria filter, out DynamicParameters param)
        {
            param = new DynamicParameters();

            param.Add(nameof(txType), txType);

            var whereClause = new StringBuilder();

            if (txType != TransactionType.Both)
                whereClause.Append($"AND tInOut.[TransactionType] = @txType ");

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

            if (filter.AddressHashes != null && filter.AddressHashes.Any())
            {
                var addressHashes = filter.AddressHashes;
                param.Add(nameof(addressHashes), addressHashes);
                whereClause.Append("AND a.[Hash] IN @addressHashes ");
            }
            
            if (filter.IsStakeReward.HasValue || filter.IsMiningReward.HasValue)
            {
                var isMiningOp = filter.IsMiningReward.HasValue ? (filter.IsMiningReward.Value ? "=" : "<>") : "";
                var isStakingOp = filter.IsStakeReward.HasValue ? (filter.IsStakeReward.Value ? "=" : "<>") : "";

                if (filter.IsStakeReward.HasValue && filter.IsMiningReward.HasValue)
                {
                    whereClause.Append($@"AND (t.[RewardType] {isMiningOp} {(int) BlockRewardType.Mining} {(filter.IsStakeReward.Value && filter.IsMiningReward.Value ? "OR" : "AND")} t.[RewardType] {isStakingOp} {(int) BlockRewardType.Staking}) ");
                }
                else if (filter.IsStakeReward.HasValue)
                    whereClause.Append($"AND t.[RewardType] {isStakingOp} {(int)BlockRewardType.Mining} ");
                else
                    whereClause.Append($"AND t.[RewardType] {isMiningOp} {(int)BlockRewardType.Staking} ");
            }

            return whereClause.ToString();
        }
    }
}