using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Data.Publish;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Data.Cache.Block
{
    public class BlockCacheBuild
    {
        private readonly IBlockCache _blockCache;
        private readonly NexusQuery _nexusQuery;
        private readonly RollingCountPublisher _countPublisher;
        private readonly RedisCommand _redisCommand;
        private readonly ILogger<BlockCacheBuild> _logger;
        private readonly BlockQuery _blockQuery;

        public BlockCacheBuild(IBlockCache blockCache, NexusQuery nexusQuery, RollingCountPublisher countPublisher, 
            RedisCommand redisCommand, ILogger<BlockCacheBuild> logger, BlockQuery blockQuery)
        {
            _blockCache = blockCache;
            _nexusQuery = nexusQuery;
            _countPublisher = countPublisher;
            _redisCommand = redisCommand;
            _logger = logger;
            _blockQuery = blockQuery;
        }

        public async Task BuildAsync()
        {
            await _blockCache.Clear();

            var nextHeight = (await _blockQuery.GetLastSyncedHeightAsync()) + 1;

            var blockCount = 0;
            var txCount = 0;

            var cache = new List<BlockDto>();

            for (var i = nextHeight; i < nextHeight + Settings.App.BlockCacheCount; i++)
            {
                var block = await _nexusQuery.GetBlockAsync(i, true);

                if (block == null)
                    break;

                await _blockCache.AddAsync(block);

                blockCount++;

                txCount += block.Transactions.Count;
            }

            await _blockCache.SaveAsync();

            _logger.LogInformation($"{txCount} transactions and {blockCount} blocks added to cache");
        }
    }
}