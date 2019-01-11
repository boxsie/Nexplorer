using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public class CacheCleanupJob : HostedService
    {
        private readonly BlockQuery _blockQuery;
        private readonly RedisCommand _redisCommand;

        public CacheCleanupJob(ILogger<CacheCleanupJob> logger, BlockQuery blockQuery, RedisCommand redisCommand)
            : base(30, logger)
        {
            _blockQuery = blockQuery;
            _redisCommand = redisCommand;
        }

        protected override async Task ExecuteAsync()
        {
            var sw = new Stopwatch();
            sw.Start();

            var lastSyncedHeight = await _blockQuery.GetLastSyncedHeightAsync();
            var lowestUnremovedHeight = await GetLowestUnremovedHeight(lastSyncedHeight);
            var cache = await GetCachedBlocks(lowestUnremovedHeight);
            
            var addressesToKeep = cache
                .Where(x => x.Height > lastSyncedHeight)
                .SelectMany(x => x.Transactions
                    .SelectMany(y => y.Inputs.Concat(y.Outputs))
                    .Select(y => y.AddressHash)
                    .Distinct())
                .ToList();

            var blocksToDelete = cache
                .Where(x => x.Height <= lastSyncedHeight)
                .ToList();

            if (!blocksToDelete.Any())
                return;

            var addressesToDelete = GetAddressesToDelete(addressesToKeep, blocksToDelete);

            foreach (var blockDto in blocksToDelete)
            {
                Logger.LogInformation($"Deleting block {blockDto.Height} from the cache");
                await _redisCommand.DeleteAsync(Settings.Redis.BuildCachedBlockKey(blockDto.Height));
            }

            foreach (var addressHash in addressesToDelete)
            {
                Logger.LogInformation($"Deleting address {addressHash} from the cache");
                await _redisCommand.DeleteAsync(Settings.Redis.BuildCachedAddressKey(addressHash));
            }

            sw.Stop();
            Logger.LogInformation($"Cache cleanup took {sw.Elapsed:c}");
        }

        private Task<BlockDto> GetCacheBlock(int height)
        {
            return _redisCommand.GetAsync<BlockDto>(Settings.Redis.BuildCachedBlockKey(height));
        }

        private async Task<int> GetLowestUnremovedHeight(int heightToDelete)
        {
            var cacheBlock = await GetCacheBlock(heightToDelete);

            while (cacheBlock != null)
            {
                cacheBlock = await GetCacheBlock(heightToDelete - 1);

                if (cacheBlock != null)
                    heightToDelete = heightToDelete - 1;
            }
            
            return heightToDelete;
        }

        private async Task<List<BlockDto>> GetCachedBlocks(int fromHeight)
        {
            var cache = new List<BlockDto>();
            var nextHeight = fromHeight;

            var cacheBlock = await GetCacheBlock(nextHeight);

            while (cacheBlock != null)
            {
                cache.Add(cacheBlock);
                nextHeight++;
                cacheBlock = await GetCacheBlock(nextHeight);
            }

            return cache;
        }

        private static IEnumerable<string> GetAddressesToDelete(IEnumerable<string> addressesToKeep, IEnumerable<BlockDto> blocksToDelete)
        {
            var addressesToDelete = new List<string>();

            foreach (var deleteBlock in blocksToDelete)
            {
                var deleteBlockAddresses = deleteBlock.Transactions
                    .SelectMany(x => x.Inputs.Concat(x.Outputs)
                        .Select(y => y.AddressHash));

                addressesToDelete.AddRange(deleteBlockAddresses.Where(hash => addressesToKeep.All(x => x != hash)));
            }

            return addressesToDelete;
        }
    }
}