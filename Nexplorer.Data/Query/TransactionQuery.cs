using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Data.Context;
using Nexplorer.Domain.Dtos;

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
    }
}