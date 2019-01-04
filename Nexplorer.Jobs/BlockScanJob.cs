using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Data.Command;
using Nexplorer.Data.Query;
using Nexplorer.Jobs.Service;

namespace Nexplorer.Jobs
{
    public class BlockScanJob : HostedService
    {
        private readonly NexusQuery _nexusQuery;
        private readonly BlockCacheCommand _cacheCommand;
        private readonly ILogger<BlockScanJob> _logger;

        private int _nextHeight = 0;

        public BlockScanJob(NexusQuery nexusQuery, BlockCacheCommand cacheCommand, ILogger<BlockScanJob> logger) 
            : base(3)
        {
            _nexusQuery = nexusQuery;
            _cacheCommand = cacheCommand;
            _logger = logger;
        }

        protected override async Task ExecuteAsync()
        {
            if (_nextHeight == 0)
                _nextHeight = await _nexusQuery.GetBlockchainHeightAsync() + 1;

            var newBlock = await _nexusQuery.GetBlockAsync(_nextHeight, false);

            while (newBlock != null)
            {
                _logger.LogInformation($"Found new block {_nextHeight}");

                var block = newBlock;

                await _cacheCommand.AddAsync(block.Height, true);

                _nextHeight = block.Height + 1;

                newBlock = await _nexusQuery.GetBlockAsync(_nextHeight, false);
            }
        }
    }
}