﻿using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Sync.Hangfire
{
    public class BlockCacheJob
    {
        private readonly ILogger<BlockCacheJob> _logger;
        private readonly NexusQuery _nexusQuery;
        private readonly RedisCommand _redisCommand;

        private const int TimeoutSeconds = 10;

        public BlockCacheJob(ILogger<BlockCacheJob> logger, NexusQuery nexusQuery, RedisCommand redisCommand)
        {
            _logger = logger;
            _nexusQuery = nexusQuery;
            _redisCommand = redisCommand;
        }

        [AutomaticRetry(Attempts = 1)]
        [DisableConcurrentExecution(TimeoutSeconds)]
        public async Task CreateAsync()
        {
            var chainHeight = await _nexusQuery.GetBlockchainHeightAsync();
            var nextHeight = chainHeight - Settings.App.BlockCacheCount;

            for (var i = 0; i <= Settings.App.BlockCacheCount; i++)
                await AddAsync(nextHeight + i, false);

            _logger.LogInformation($"{Settings.App.BlockCacheCount} blocks added to cache");
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task AddAsync(int blockHeight, bool publish)
        {
            try
            {
                if (blockHeight == 0)
                    return;

                if (await CacheBlockExistsAsync(blockHeight))
                {
                    _logger.LogInformation($"Block {blockHeight} is already in the cache");
                    return;
                }
                
                var cacheHeight = await _redisCommand.GetAsync<int>(Settings.Redis.CachedHeight);

                if (cacheHeight > 0)
                {
                    while (blockHeight > cacheHeight + 1)
                    {
                        _logger.LogInformation($"Found new block {blockHeight}");

                        var prevBlock = await GetBlockAsync(cacheHeight + 1);

                        await _redisCommand.SetAsync(Settings.Redis.BuildCachedBlockKey(prevBlock.Height), prevBlock);

                        cacheHeight++;
                    }
                }
                
                var block = await GetBlockAsync(blockHeight);

                await _redisCommand.SetAsync(Settings.Redis.BuildCachedBlockKey(block.Height), block);

                if (cacheHeight < blockHeight)
                    await _redisCommand.SetAsync(Settings.Redis.CachedHeight, blockHeight);

                if (publish)
                    BackgroundJob.Enqueue<BlockPublishJob>(x => x.PublishAsync(block.Height));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);

                throw;
            }
        }

        private async Task<bool> CacheBlockExistsAsync(int height)
        {
            return await _redisCommand.GetAsync<BlockDto>(Settings.Redis.BuildCachedBlockKey(height)) != null;
        }

        private async Task<BlockDto> GetBlockAsync(int height)
        {
            var block = await _nexusQuery.GetBlockAsync(height, true);

            if (block == null)
            {
                _logger.LogWarning($"Nexus returned null for {height}");

                throw new NullReferenceException($"Nexus returned null for {height}");
            }

            return block;
        }
    }
}