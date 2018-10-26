using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
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
                .Include(x => x.Inputs)
                .ThenInclude(x => x.Address)
                .Include(x => x.Outputs)
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

        public async Task<FilterResult<TransactionDto>> GetTransactionsFilteredAsync(TransactionFilterCriteria filter, int start, int count, bool countResults, int? maxResults = null)
        {
            var min = filter.MinAmount ?? 0;
            var max = filter.MaxAmount ?? double.MaxValue;
            var fromHeight = filter.HeightFrom ?? 0;
            var toHeight = filter.HeightTo ?? int.MaxValue;
            var fromDate = filter.UtcFrom ?? DateTime.MinValue;
            var toDate = filter.UtcTo ?? DateTime.MaxValue;

            var tables = _nexusDb.Transactions
                .Include(x => x.Inputs)
                .ThenInclude(x => x.Address)
                .Include(x => x.Outputs)
                .ThenInclude(x => x.Address)
                .Include(x => x.Block);

            Expression<Func<Transaction, bool>> query = x => 
                x.Amount >= min && x.Amount <= max &&
                x.Block.Height >= fromHeight &&
                x.BlockHeight <= toHeight &&
                x.Timestamp >= fromDate &&
                x.Timestamp <= toDate &&
                MatchAddressHash(x.Inputs.Select(y => y.Address.Hash).ToArray(), filter.FromAddressHashes) &&
                MatchAddressHash(x.Outputs.Select(y => y.Address.Hash).ToArray(), filter.ToAddressHashes);

            var resultCount = countResults 
                ? await tables.CountAsync(query)
                : -1;

            var txs = await tables
                .Where(query)
                .Skip(start)
                .Take(count)
                .ToListAsync();

            return new FilterResult<TransactionDto>
            {
                ResultCount = resultCount,
                Results = txs.Select(x => _mapper.Map<TransactionDto>(x)).ToList()
            };
        }

        private bool MatchAddressHash(string[] hashesOne, string[] hashesTwo)
        {
            if (hashesOne == null || !hashesOne.Any() || hashesTwo == null || !hashesTwo.Any())
                return false;

            return hashesOne.Any(x => hashesTwo.Any(y => y == x));
        }
    }
}