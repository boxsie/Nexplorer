using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Data.Publish
{
    public class LatestBlockPublisher
    {
        private readonly RedisCommand _redisCommand;
        private readonly ILogger<LatestBlockPublisher> _logger;
        private readonly NexusQuery _nexusQuery;

        public LatestBlockPublisher(RedisCommand redisCommand, ILogger<LatestBlockPublisher> logger, NexusQuery nexusQuery)
        {
            _redisCommand = redisCommand;
            _logger = logger;
            _nexusQuery = nexusQuery;
        }

        public async Task PublishLatestDataAsync(BlockDto block)
        {
            var blockLite = new BlockLiteDto(block);

            await _redisCommand.PublishAsync(Settings.Redis.NewBlockPubSub, blockLite);

            if (block.Transactions == null || !block.Transactions.Any())
                return;

            foreach (var tx in block.Transactions)
                await _redisCommand.PublishAsync(Settings.Redis.NewTransactionPubSub,
                    new TransactionLiteDto(tx, block.Height));

            _logger.LogInformation($"Published block {block.Height}");

            var miningInfo = await _nexusQuery.GetMiningInfoAsync();
            await _redisCommand.SetAsync(Settings.Redis.MiningInfoLatest, miningInfo);
        }
    }
}
