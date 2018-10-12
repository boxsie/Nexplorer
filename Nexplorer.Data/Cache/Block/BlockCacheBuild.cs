using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Data.Publish;
using Nexplorer.Data.Query;

namespace Nexplorer.Data.Cache.Block
{
    public class BlockCacheBuild
    {
        private readonly IBlockCache _blockCache;
        private readonly NexusQuery _nexusQuery;
        private readonly RollingCountPublisher _countPublisher;
        private readonly RedisCommand _redisCommand;
        private readonly ILogger<BlockCacheBuild> _logger;

        public BlockCacheBuild(IBlockCache blockCache, NexusQuery nexusQuery, RollingCountPublisher countPublisher, RedisCommand redisCommand, ILogger<BlockCacheBuild> logger)
        {
            _blockCache = blockCache;
            _nexusQuery = nexusQuery;
            _countPublisher = countPublisher;
            _redisCommand = redisCommand;
            _logger = logger;
        }

        public async Task BuildAsync(int syncedHeight)
        {
            await _blockCache.Clear();

            var blockCount = 0;
            var txCount = 0;

            for (var i = syncedHeight; i <= syncedHeight + Settings.App.BlockCacheCount; i++)
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

            await _countPublisher.PublishRollingCountAsync();

            var miningInfo = await _nexusQuery.GetMiningInfoAsync();
            await _redisCommand.SetAsync(Settings.Redis.MiningInfoLatest, miningInfo);
        }
    }
}