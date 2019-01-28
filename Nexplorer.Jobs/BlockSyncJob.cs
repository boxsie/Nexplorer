using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Command;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Jobs.Service;

namespace Nexplorer.Jobs
{
    public class BlockSyncJob : HostedService
    {
        private readonly NexusQuery _nexusQuery;
        private readonly BlockQuery _blockQuery;
        private readonly BlockPublishCommand _blockPublish;
        private readonly BlockInsertCommand _blockInsert;
        private readonly AddressAggregatorCommand _addressAggregator;

        public BlockSyncJob(ILogger<BlockSyncJob> logger, NexusQuery nexusQuery, BlockQuery blockQuery, BlockPublishCommand blockPublish,
            BlockInsertCommand blockInsert, AddressAggregatorCommand addressAggregator)
            : base(3, logger)
        {
            _nexusQuery = nexusQuery;
            _blockQuery = blockQuery;
            _blockPublish = blockPublish;
            _blockInsert = blockInsert;
            _addressAggregator = addressAggregator;
        }

        protected override async Task ExecuteAsync()
        {
            var lastHeight = await _blockQuery.GetLastHeightAsync();
            var nextBlock = await GetBlockAsync(lastHeight + 1);

            if (nextBlock == null)
            {
                Logger.LogInformation("Block sync found no blocks to sync.");
                return;
            }

            while (nextBlock != null)
            {
                Logger.LogInformation($"Found new block {nextBlock.Height}");

                await AddBlockAsync(nextBlock);

                await AggregateAddressesAsync(nextBlock);

                await _blockPublish.PublishAsync(nextBlock);

                nextBlock = await GetBlockAsync(nextBlock.Height + 1);
            }
        }

        private Task<BlockDto> GetBlockAsync(int height)
        {
            return _nexusQuery.GetBlockAsync(height, true);
        }

        private async Task AddBlockAsync(BlockDto blockDto)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var block = await _blockInsert.InsertBlockAsync(blockDto);

            if (block == null)
                throw new NullReferenceException("Something failed inserting the new block");

            stopwatch.Stop();
            Logger.LogInformation($"Block {block.Height} synced in {stopwatch.Elapsed:g}");
        }

        private async Task AggregateAddressesAsync(BlockDto blockDto)
        {
            var stopwatch = new Stopwatch();

            Logger.LogInformation($"Syncing address from block {blockDto.Height}...");

            stopwatch.Start();

            await _addressAggregator.AggregateAddressesAsync(blockDto);

            stopwatch.Stop();

            Logger.LogInformation($"Address synced in {stopwatch.Elapsed:g}");
        }
    }
}
