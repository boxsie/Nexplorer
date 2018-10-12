using System;
using System.Linq;
using System.Threading.Tasks;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Cache;
using Nexplorer.Data.Cache.Block;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Data.Query;

namespace Nexplorer.Data.Publish
{
    public class RollingCountPublisher
    {
        private readonly BlockQuery _blockQuery;
        private readonly IBlockCache _blockCache;
        private readonly RedisCommand _redisCommand;

        public RollingCountPublisher(BlockQuery blockQuery, IBlockCache blockCache, RedisCommand redisCommand)
        {
            _blockQuery = blockQuery;
            _blockCache = blockCache;
            _redisCommand = redisCommand;
        }

        public async Task PublishRollingCountAsync()
        {
            var blockCount = await GetLastDayBlockCount();

            await _redisCommand.SetAsync(Settings.Redis.BlockCount24Hours, blockCount);
            await _redisCommand.PublishAsync(Settings.Redis.BlockCount24Hours, blockCount);

            var txCount = await GetLastDayTransactionCount();

            await _redisCommand.SetAsync(Settings.Redis.TransactionCount24Hours, txCount);
            await _redisCommand.PublishAsync(Settings.Redis.TransactionCount24Hours, txCount);
        }

        private async Task<int> GetLastDayBlockCount()
        {
            var cache = await _blockCache.GetBlockLiteCacheAsync();

            var lastNonOrphanBlockTime = cache.FirstOrDefault(x => x.TimeUtc != new DateTime())?.TimeUtc
                ?? (await _blockQuery.GetLastBlockAsync()).TimeUtc;

            return await _blockQuery.GetBlockCount(lastNonOrphanBlockTime, 1) + cache.Count;
        }

        private async Task<int> GetLastDayTransactionCount()
        {
            var cache = await _blockCache.GetTransactionLiteCacheAsync();

            var lastNonOrphanTxTime = cache.FirstOrDefault(x => x.TimeUtc != new DateTime())?.TimeUtc
                                     ?? (await _blockQuery.GetLastTransaction()).TimeUtc;

            return await _blockQuery.GetTransactionCount(lastNonOrphanTxTime, 1) + cache.Count;
        }
    }
}