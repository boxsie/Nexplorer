using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Tools.Jobs
{
    public class BlockCacheCleanupJob
    {
        public static readonly TimeSpan JobInterval = TimeSpan.FromMinutes(1);

        private readonly ILogger<BlockCacheCleanupJob> _logger;
        private readonly BlockQuery _blockQuery;
        private readonly RedisCommand _redisCommand;

        public BlockCacheCleanupJob(ILogger<BlockCacheCleanupJob> logger, BlockQuery blockQuery, RedisCommand redisCommand)
        {
            _logger = logger;
            _blockQuery = blockQuery;
            _redisCommand = redisCommand;
        }

        public async Task CleanUpAsync()
        {
            var heightToDelete = await _blockQuery.GetLastSyncedHeightAsync();
            var cacheBlock = await GetCacheBlock(heightToDelete);

            while (cacheBlock != null)
            {
                _logger.LogInformation($"Deleting block {heightToDelete} from the cache");

                await _redisCommand.DeleteAsync(Settings.Redis.BuildCachedBlockKey(heightToDelete));

                heightToDelete--;
                cacheBlock = await GetCacheBlock(heightToDelete);
            }

            BackgroundJob.Schedule<BlockCacheCleanupJob>(x => x.CleanUpAsync(), JobInterval);
        }

        private Task<BlockDto> GetCacheBlock(int height)
        {
            return _redisCommand.GetAsync<BlockDto>(Settings.Redis.BuildCachedBlockKey(height));
        }
    }
}