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
using Nexplorer.Data.Context;
using Nexplorer.Data.Services;
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
        private readonly CacheService _cache;
        
        public TransactionQuery(NexusDb nexusDb, IMapper mapper, RedisCommand redisCommand, CacheService cache)
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

        public async Task<int> GetTransactionCountLastDay()
        {
            return await _redisCommand.GetAsync<int>(Settings.Redis.TransactionCount24Hours);
        }

        public async Task<FilterResult<TransactionLiteDto>> GetTransactionsFilteredAsync(TransactionFilterCriteria filter, int start, int count, bool countResults, int? maxResults = null)
        {
            const string from = @"
                FROM [dbo].[Transaction] t 
                WHERE 1 = 1 ";
            
            var where = BuildWhereClause(filter, out var param);

            var sqlOrderBy = "ORDER BY ";

            switch (filter.OrderBy)
            {
                case OrderTransactionsBy.LowestAmount:
                    sqlOrderBy += "t.[Amount] ";
                    break;
                case OrderTransactionsBy.HighestAmount:
                    sqlOrderBy += "t.[Amount] DESC ";
                    break;
                case OrderTransactionsBy.LeastRecent:
                    sqlOrderBy += "t.[Timestamp] ";
                    break;
                case OrderTransactionsBy.MostRecent:
                    sqlOrderBy += "t.[Timestamp] DESC ";
                    break;
                default:
                    sqlOrderBy += "t.[Timestamp] DESC ";
                    break;
            }

            var sqlQ = $@"SELECT
                          t.[TransactionId],
                          t.[Hash] AS TransactionHash,
                          t.[BlockHeight],
                          t.[Timestamp],
                          t.[Amount],
                          t.[TransactionType],
                          (
	                          SELECT 
	                          COUNT(*)
	                          FROM [dbo].[TransactionInputOutput] tInOut 
	                          WHERE tInOut.[TransactionId] = t.[TransactionId]
	                          AND tInOut.[TransactionInputOutputType] = 0
                          ) AS TransactionInputCount,
                          (
	                          SELECT 
	                          COUNT(*)
	                          FROM [dbo].[TransactionInputOutput] tInOut 
	                          WHERE tInOut.[TransactionId] = t.[TransactionId]
	                          AND tInOut.[TransactionInputOutputType] = 1
                          ) AS TransactionOutputCount
                          {from} 
                          {where} 
                          AND t.[Amount] > 0 
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
                var cacheTxs = FilterCacheBlocks(await _cache.GetBlocksAsync(), filter).ToList();
                
                param.Add(nameof(count), count);
                param.Add(nameof(start), start);
                param.Add(nameof(maxResults), maxResults ?? int.MaxValue);
                
                using (var multi = await sqlCon.QueryMultipleAsync(string.Concat(sqlQ, sqlC), param))
                {
                    var results = cacheTxs.Concat(await multi.ReadAsync<TransactionLiteDto>());
                    var resultCount = countResults
                        ? cacheTxs.Count + (int)(await multi.ReadAsync<int>()).FirstOrDefault()
                        : -1;

                    switch (filter.OrderBy)
                    {
                        case OrderTransactionsBy.MostRecent:
                            results = results.OrderByDescending(x => x.Timestamp);
                            break;
                        case OrderTransactionsBy.LeastRecent:
                            results = results.OrderBy(x => x.Timestamp);
                            break;
                        case OrderTransactionsBy.HighestAmount:
                            results = results.OrderByDescending(x => x.Amount);
                            break;
                        case OrderTransactionsBy.LowestAmount:
                            results = results.OrderBy(x => x.Amount);
                            break;
                    }

                    var filterResult = new FilterResult<TransactionLiteDto>()
                    {
                        ResultCount = resultCount > 1000 ? 1000 : resultCount,
                        Results = results.Take(count).ToList()
                    };

                    return filterResult;
                }
            }
        }

        private bool MatchAddressHash(string[] hashesOne, string[] hashesTwo)
        {
            if (hashesOne == null || !hashesOne.Any() || hashesTwo == null || !hashesTwo.Any())
                return false;

            return hashesOne.Any(x => hashesTwo.Any(y => y == x));
        }

        private static IEnumerable<TransactionLiteDto> FilterCacheBlocks(IEnumerable<BlockDto> blocks, TransactionFilterCriteria filter)
        {
            return blocks.SelectMany(x => x.Transactions
                .Where(y => (!filter.TxType.HasValue || y.TransactionType == filter.TxType) &&
                            (!filter.MinAmount.HasValue || y.Amount >= filter.MinAmount) &&
                            (!filter.MaxAmount.HasValue || y.Amount <= filter.MaxAmount) &&
                            (!filter.HeightFrom.HasValue || y.BlockHeight >= filter.HeightFrom) &&
                            (!filter.HeightTo.HasValue || y.BlockHeight <= filter.HeightTo) &&
                            (!filter.UtcFrom.HasValue || y.Timestamp >= filter.UtcFrom) &&
                            (!filter.UtcTo.HasValue || y.Timestamp <= filter.UtcTo))
                .Select(y => new TransactionLiteDto(y)));
        }

        private static string BuildWhereClause(TransactionFilterCriteria filter, out DynamicParameters param)
        {
            param = new DynamicParameters();

            var whereClause = new StringBuilder();

            if (filter.TxType.HasValue)
            {
                var txType = (int)filter.TxType.Value;
                param.Add(nameof(txType), txType);
                whereClause.Append($"AND t.[TransactionType] = @txType ");
            }

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
                whereClause.Append($"AND t.[BlockHeight] >= @fromHeight ");
            }

            if (filter.HeightTo.HasValue)
            {
                var toHeight = filter.HeightTo.Value;
                param.Add(nameof(toHeight), toHeight);
                whereClause.Append($"AND t.[BlockHeight] <= @toHeight ");
            }

            if (filter.UtcFrom.HasValue)
            {
                var fromDate = filter.UtcFrom.Value;
                param.Add(nameof(fromDate), fromDate);
                whereClause.Append($"AND t.[Timestamp] >= @fromDate ");
            }

            if (filter.UtcTo.HasValue)
            {
                var toDate = filter.UtcTo.Value;
                param.Add(nameof(toDate), toDate);
                whereClause.Append($"AND t.[Timestamp] <= @toDate ");
            }

            return whereClause.ToString();
        }
    }
}