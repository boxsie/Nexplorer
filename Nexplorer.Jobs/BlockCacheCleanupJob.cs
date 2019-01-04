using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Jobs.Service;

namespace Nexplorer.Jobs
{
    public class BlockCacheCleanupJob : HostedService
    {
        private readonly ILogger<BlockCacheCleanupJob> _logger;
        private readonly BlockQuery _blockQuery;
        private readonly RedisCommand _redisCommand;

        public BlockCacheCleanupJob(ILogger<BlockCacheCleanupJob> logger, BlockQuery blockQuery, RedisCommand redisCommand) 
            : base(60)
        {
            _logger = logger;
            _blockQuery = blockQuery;
            _redisCommand = redisCommand;
        }

        protected override async Task ExecuteAsync()
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
        }

        private Task<BlockDto> GetCacheBlock(int height)
        {
            return _redisCommand.GetAsync<BlockDto>(Settings.Redis.BuildCachedBlockKey(height));
        }
    }
}