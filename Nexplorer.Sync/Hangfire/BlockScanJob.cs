using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Nexplorer.Data.Query;

namespace Nexplorer.Sync.Hangfire
{
    public class BlockScanJob
    {
        public static readonly TimeSpan JobInterval = TimeSpan.FromSeconds(3);

        private readonly NexusQuery _nexusQuery;
        private readonly ILogger<BlockScanJob> _logger;

        private const int TimeoutSeconds = 10;

        public BlockScanJob(NexusQuery nexusQuery, ILogger<BlockScanJob> logger)
        {
            _nexusQuery = nexusQuery;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 1)]
        [DisableConcurrentExecution(TimeoutSeconds)]
        public async Task ScanAsync(int? nextHeight)
        {
            var newBlock = await _nexusQuery.GetBlockAsync(nextHeight, false);

            while (newBlock != null)
            {
                _logger.LogInformation($"Found new block {nextHeight}");

                var block = newBlock;

                BackgroundJob.Enqueue<BlockCacheJob>(x => x.AddAsync(block.Height, true));

                nextHeight = block.Height + 1;

                newBlock = await _nexusQuery.GetBlockAsync(nextHeight, false);
            }

            BackgroundJob.Schedule<BlockScanJob>(x => x.ScanAsync(nextHeight), JobInterval);
        }
    }
}