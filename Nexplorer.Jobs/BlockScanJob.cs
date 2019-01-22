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

            var blockNeedsRefresh = false;

            if (block.Hash != blockDto.Hash)
            {
                blockNeedsRefresh = true;
                Logger.LogWarning($"Block {block.Height} has a mismatched hash");
            }
            else if (block.Transactions.Count != blockDto.Transactions.Count || !block.Transactions.All(x => blockDto.Transactions.Any(y => x.Hash == y.Hash)))
            {
                blockNeedsRefresh = true;
                Logger.LogWarning($"Block {block.Height} has a mismatched transaction hashes");
            }
            else if (!await _nexusQuery.IsBlockHashOnChain(block.Hash))
            {
                blockNeedsRefresh = true;
                Logger.LogWarning($"Block {block.Height} is being reported as an orphan by the node");
            }

            if (blockNeedsRefresh)
            {
                Logger.LogWarning($"Reverting address aggregation from block {block.Height} addresses");
                await _addressAggregator.RevertAggregate(block);

                Logger.LogWarning($"Deleting block, transaction and io data for block {block.Height}");
                await _blockDelete.DeleteBlockAsync(block);

                Logger.LogWarning($"Inserting new data for block {block.Height}");
                await _blockInsert.InsertBlockAsync(blockDto);
                await _addressAggregator.AggregateAddressesAsync(blockDto);
            }
        }
    }
}