using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Context;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.User;

namespace Nexplorer.Data.Command
{
    public class BlockCacheCommand
    {
        private readonly ILogger<BlockCacheCommand> _logger;
        private readonly NexusQuery _nexusQuery;
        private readonly RedisCommand _redisCommand;
        private readonly BlockPublishCommand _blockPublish;

        public BlockCacheCommand(ILogger<BlockCacheCommand> logger, NexusQuery nexusQuery, RedisCommand redisCommand, BlockPublishCommand blockPublish)
        {
            _logger = logger;
            _nexusQuery = nexusQuery;
            _redisCommand = redisCommand;
            _blockPublish = blockPublish;
        }

        public async Task<List<BlockDto>> CreateAsync()
        {
            var chainHeight = await _nexusQuery.GetBlockchainHeightAsync();
            var nextHeight = chainHeight - Settings.App.BlockCacheCount;

            var cache = new List<BlockDto>();

            for (var i = 0; i <= Settings.App.BlockCacheCount; i++)
                cache.Add(await AddAsync(nextHeight + i, false));

            _logger.LogInformation($"{Settings.App.BlockCacheCount} blocks added to cache");

            return cache;
        }

        public async Task<BlockDto> AddAsync(int blockHeight, bool publish)
        {
            try
            {
                if (blockHeight == 0)
                    return null;

                if (await CacheBlockExistsAsync(blockHeight))
                {
                    _logger.LogInformation($"Block {blockHeight} is already in the cache");
                    return null;
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
                    await _blockPublish.PublishAsync(block.Height);

                return block;
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
