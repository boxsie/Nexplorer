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
                
                param.Add(nameof(start), start);
                param.Add(nameof(count), count);
                param.Add(nameof(maxResults), maxResults ?? int.MaxValue);

                if (countResults)
                {
                    using (var multi = await sqlCon.QueryMultipleAsync(string.Concat(sqlQ, sqlC), param))
                    {
                        results.Results = MapTransactions(await multi.ReadAsync());
                        results.ResultCount = (int)(await multi.ReadAsync<int>()).FirstOrDefault();
                    }
                }
                else
                {
                    results.Results = MapTransactions(await sqlCon.QueryAsync(sqlQ, param));
                    results.ResultCount = -1;
                }

                return results;
            }
        }

        public async Task<Dictionary<int, List<TransactionAddressDto>>> GetTransactionAddresses(IEnumerable<int> transactionIds)
        {
            return await _nexusDb.TransactionInputOutput
                .Include(x => x.Transaction)
                .Include(x => x.Address)
                .Where(x => transactionIds.Any(y => y == x.Transaction.TransactionId))
                .GroupBy(x => x.TransactionId, x => new TransactionAddressDto
                {
                    AddressHash = x.Address.Hash,
                    TransactionId = x.TransactionId,
                    TransactionType = x.TransactionType
                })
                .ToDictionaryAsync(x => x.Key, x => x.Select(y => y).ToList());
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

            return whereClause.ToString();
        }
    }
}