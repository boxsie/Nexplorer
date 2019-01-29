using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Data.Command;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
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
            var dbBlock = await _blockQuery.GetBlockAsync(blockHeight, true);
            var nxsBlock = await _nexusQuery.GetBlockAsync(blockHeight, true);

            if (nxsBlock == null)
            {
                var retryCounter = 0;

                while (retryCounter < 3)
                {
                    nxsBlock = await _nexusQuery.GetBlockAsync(blockHeight, true);

                    if (nxsBlock != null)
                        break;

                    await Task.Delay(1000);

                    retryCounter++;
                }

                if (nxsBlock == null)
                {
                    Logger.LogWarning($"Nexus block {blockHeight} is returning null");
                    return;
                }
            }

            var nullDbBlock = dbBlock == null;

            if (nullDbBlock || dbBlock.Hash != nxsBlock.Hash)
            {
                BlockDto deleteBlock;

                if (nullDbBlock)
                {
                    deleteBlock = new BlockDto { Height = blockHeight };

                    Logger.LogWarning($"Block {blockHeight} is returning null from the database");
                }
                else
                {
                    deleteBlock = dbBlock;

                    Logger.LogWarning($"Block {blockHeight} has a mismatched hash");
                    Logger.LogWarning($"Reverting address aggregation from block {blockHeight} addresses");

                    await _addressAggregator.RevertAggregate(deleteBlock);
                }

                Logger.LogWarning($"Deleting block, transaction and io data for block {blockHeight}");
                await _blockDelete.DeleteBlockAsync(deleteBlock);

                Logger.LogWarning($"Inserting new data for block {blockHeight}");
                await _blockInsert.InsertBlockAsync(nxsBlock);
                await _addressAggregator.AggregateAddressesAsync(nxsBlock);
            }
        }
    }
}