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
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Data.Query
{
    public class BlockQuery
    {
        private readonly BlockCacheService _cache;
        private readonly RedisCommand _redisCommand;
        private readonly NexusDb _nexusDb;
        private readonly IMapper _mapper;

        public BlockQuery(BlockCacheService cache, RedisCommand redisCommand, NexusDb nexusDb, IMapper mapper)
        {
            _cache = cache;
            _redisCommand = redisCommand;
            _nexusDb = nexusDb;
            _mapper = mapper;
        }

        public async Task<int> GetLastHeightAsync()
        {
            var cacheHeight = await _cache.GetLastHeightAsync();

            if (cacheHeight > 0)
                return cacheHeight;

            return await GetLastSyncedHeightAsync();
        }

        public async Task<int> GetLastSyncedHeightAsync()
        {
            return await _nexusDb.Blocks.OrderByDescending(x => x.Height)
                .Select(x => x.Height)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetChannelHeight(BlockChannels channel, int height = 0)
        {
            var count = await  _cache.GetChannelHeightAsync(channel, height);

            count += height == 0 
                ? await _nexusDb.Blocks.CountAsync(x => x.Channel == (int)channel) 
                : await _nexusDb.Blocks.CountAsync(x => x.Channel == (int)channel && x.Height <= height);

            return count;
        }

        public async Task<BlockDto> GetBlockAsync(int height)
        {
            var cacheBlock = await _cache.GetBlockAsync(height);

            if (cacheBlock != null)
                return cacheBlock;
            
            var block = await _nexusDb.Blocks.Where(x => x.Height == height)
                .Include(x => x.Transactions)
                    .ThenInclude(x => x.Inputs)
                        .ThenInclude(x => x.Address)
                .Include(x => x.Transactions)
                    .ThenInclude(x => x.Outputs)
                        .ThenInclude(x => x.Address)
                .FirstOrDefaultAsync();

            return _mapper.Map<BlockDto>(block);
        }

        public async Task<BlockDto> GetBlockAsync(string hash)
        {
            var cacheBlock = await _cache.GetBlockAsync(hash);

            if (cacheBlock != null)
                return cacheBlock;
            
            var block = await _nexusDb.Blocks.Where(x => x.Hash == hash)
                .Include(x => x.Transactions)
                    .ThenInclude(x => x.Inputs)
                    .ThenInclude(x => x.Address)
                .Include(x => x.Transactions)
                    .ThenInclude(x => x.Outputs)
                    .ThenInclude(x => x.Address)
                .FirstOrDefaultAsync() ?? await _nexusDb.Blocks.Where(x => x.Hash.StartsWith(hash))
                    .Include(x => x.Transactions)
                        .ThenInclude(x => x.Inputs)
                        .ThenInclude(x => x.Address)
                    .Include(x => x.Transactions)
                        .ThenInclude(x => x.Outputs)
                        .ThenInclude(x => x.Address)
                    .FirstOrDefaultAsync();

            return _mapper.Map<BlockDto>(block);
        }

        public async Task<BlockDto> GetBlockAsync(DateTime time)
        {
            var block = await _nexusDb.Blocks.OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync(x => x.Timestamp < time);

            return _mapper.Map<BlockDto>(block);
        }

        public async Task<string> GetBlockHashAsync(int height)
        {
            var cacheBlockHash = await _cache.GetBlockHashAsync(height);

            if (cacheBlockHash != null)
                return cacheBlockHash;
            
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

        public async Task<Block> GetLastBlockAsync()
        {
            return await _nexusDb.Blocks.LastAsync();
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

        public async Task<IEnumerable<BlockLiteDto>> GetNewBlockCacheAsync()
        {
            return await _redisCommand.GetAsync<IEnumerable<BlockLiteDto>>(Settings.Redis.BlockLiteCache) 
                ?? new List<BlockLiteDto>();
        }

        public async Task<int> GetBlockCountLastDayAsync()
        {
            return await _redisCommand.GetAsync<int>(Settings.Redis.BlockCount24Hours);
        }
    }
}
