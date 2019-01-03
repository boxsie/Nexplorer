using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Sync.Hangfire
{
    public class BlockPublishJob
    {
        private readonly ILogger<BlockPublishJob> _logger;
        private readonly RedisCommand _redisCommand;
        private readonly NexusQuery _nexusQuery;

        public BlockPublishJob(ILogger<BlockPublishJob> logger, RedisCommand redisCommand, NexusQuery nexusQuery)
        {
            _logger = logger;
            _redisCommand = redisCommand;
            _nexusQuery = nexusQuery;
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task PublishAsync(int blockHeight)
        {
            try
            {
                if (blockHeight == 0)
                    return;

                var block = await _redisCommand.GetAsync<BlockDto>(Settings.Redis.BuildCachedBlockKey(blockHeight));

                if (block == null)
                {
                    _logger.LogWarning($"Block {blockHeight} is missing from the cache");

                    throw new NullReferenceException($"Block {blockHeight} is missing from the cache");
                }

                var blockLite = new BlockLiteDto(block);

                await _redisCommand.PublishAsync(Settings.Redis.NewBlockPubSub, blockLite);

                if (block.Transactions == null || !block.Transactions.Any())
                    return;

                foreach (var tx in block.Transactions)
                    await _redisCommand.PublishAsync(Settings.Redis.NewTransactionPubSub, new TransactionLiteDto(tx, block.Height, 1));

                _logger.LogInformation($"Published block {block.Height}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
