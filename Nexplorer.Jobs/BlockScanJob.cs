using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Data.Command;
using Nexplorer.Data.Query;
using Nexplorer.Jobs.Service;

namespace Nexplorer.Jobs
{
    public class BlockScanJob : HostedService
    {
        private readonly NexusQuery _nexusQuery;
        private readonly BlockQuery _blockQuery;
        private readonly BlockInsertCommand _blockInsert;
        private readonly AddressAggregatorCommand _addressAggregator;
        private readonly BlockDeleteCommand _blockDelete;

        private int _nextHeight = 0;
        private int _startHeight = 0;

        public BlockScanJob(ILogger<BlockScanJob> logger, NexusQuery nexusQuery, BlockQuery blockQuery, BlockInsertCommand blockInsert, 
            AddressAggregatorCommand addressAggregator, BlockDeleteCommand blockDelete) 
            : base(3, logger)
        {
            _nexusQuery = nexusQuery;
            _blockQuery = blockQuery;
            _blockInsert = blockInsert;
            _addressAggregator = addressAggregator;
            _blockDelete = blockDelete;
        }

        protected override async Task ExecuteAsync()
        {
            if (_nextHeight == 0 || _nextHeight < _startHeight - Settings.App.BlockScanDepth)
            {
                _nextHeight = await _blockQuery.GetLastHeightAsync();
                _startHeight = _nextHeight;
            }

            var nextHeightAdditional = _nextHeight - (int)(Settings.App.BlockScanDepth * 0.5f);

            Logger.LogInformation($"Scanning block {_nextHeight} and {nextHeightAdditional} for mismatches {(_startHeight - _nextHeight) + 1}/{Settings.App.BlockScanDepth}");

            await ScanAndRepair(_nextHeight);
            await ScanAndRepair(nextHeightAdditional);

            _nextHeight--;
        }

        private async Task ScanAndRepair(int blockHeight)
        {
            var block = await _blockQuery.GetBlockAsync(_nextHeight, true);
            var blockDto = await _nexusQuery.GetBlockAsync(_nextHeight, true);

            if (blockDto == null)
            {
                var retryCounter = 0;

                while (retryCounter < 3)
                {
                    blockDto = await _nexusQuery.GetBlockAsync(_nextHeight, true);

                    if (blockDto != null)
                        break;

                    await Task.Delay(1000);

                    retryCounter++;
                }

                if (blockDto == null)
                {
                    Logger.LogWarning($"Nexus block {_nextHeight} is returning null");
                    return;
                }
            }
            
            if (block == null || block.Hash != blockDto.Hash)
            {
                Logger.LogWarning(block == null
                    ? $"Block {_nextHeight} is returning null from the database"
                    : $"Block {_nextHeight} has a mismatched hash");

                Logger.LogWarning($"Reverting address aggregation from block {_nextHeight} addresses");
                await _addressAggregator.RevertAggregate(block);

                Logger.LogWarning($"Deleting block, transaction and io data for block {_nextHeight}");
                await _blockDelete.DeleteBlockAsync(block);

                Logger.LogWarning($"Inserting new data for block {_nextHeight}");
                await _blockInsert.InsertBlockAsync(blockDto);
                await _addressAggregator.AggregateAddressesAsync(blockDto);
            }
        }
    }
}