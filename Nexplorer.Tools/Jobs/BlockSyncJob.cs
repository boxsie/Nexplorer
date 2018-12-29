using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Command;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Tools.Jobs
{
    public class BlockSyncJob
    {
        private readonly ILogger<BlockSyncJob> _logger;
        private readonly NexusQuery _nexusQuery;
        private readonly BlockQuery _blockQuery;

        private const int TimeoutSeconds = 10;
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(1);

        public BlockSyncJob(ILogger<BlockSyncJob> logger, NexusQuery nexusQuery, BlockQuery blockQuery, RedisCommand redisCommand)
        {
            _logger = logger;
            _nexusQuery = nexusQuery;
            _blockQuery = blockQuery;
        }

        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(TimeoutSeconds)]
        public async Task SyncLatestAsync()
        {
            var lastSyncedHeight = await _blockQuery.GetLastSyncedHeightAsync();
            var lastSyncedBlock = await _blockQuery.GetBlockAsync(lastSyncedHeight, false);
            var lastCachedHeight = await _blockQuery.GetLastHeightAsync();
            var syncDelta = (lastCachedHeight - lastSyncedHeight) - Settings.App.BlockCacheCount;

            if (syncDelta <= 0)
            {
                _logger.LogInformation("Block sync found no blocks to sync.");
                BackgroundJob.Schedule<BlockSyncJob>(x => x.SyncLatestAsync(), _syncInterval);
                return;
            }

            var saveCount = syncDelta > Settings.App.BulkSaveCount 
                ? Settings.App.BulkSaveCount 
                : syncDelta;

            var newBlockDtos = new List<BlockDto>();
            var orphanBlocks = new List<BlockDto>();
            
            var nextBlock = await _nexusQuery.GetBlockAsync(lastSyncedBlock.Hash, false);
            var nextBlockHash = nextBlock.NextBlockHash;

            for (var i = 0; i < saveCount; i++)
            {
                if (!await _nexusQuery.IsBlockHashOnChain(nextBlockHash))
                {
                    var msg = $"Orphan block found at height {nextBlock.Height + 1} with hash {nextBlockHash}";

                    _logger.LogCritical(msg);

                    throw new Exception(msg);
                }

                nextBlock = await _nexusQuery.GetBlockAsync(nextBlockHash, true);

                newBlockDtos.Add(nextBlock);

                nextBlockHash = nextBlock.NextBlockHash;
            }

            _logger.LogInformation($"Syncing {saveCount} blocks from {lastSyncedHeight} - {lastSyncedHeight + saveCount}...");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var newBlocks = await newBlockDtos.InsertBlocksAsync();

            stopwatch.Stop();
            _logger.LogInformation($"{saveCount} blocks synced in {stopwatch.Elapsed:g}");

            _logger.LogInformation($"Syncing addresses from new blocks...");
            stopwatch.Restart();

            using (var addAgg = new AddressAggregator())
                await addAgg.AggregateAddresses(newBlocks);

            stopwatch.Stop();
            _logger.LogInformation($"Addresses synced in {stopwatch.Elapsed:g}");

            BackgroundJob.Schedule<BlockSyncJob>(x => x.SyncLatestAsync(), _syncInterval);

            //await _nexusDb.OrphanBlocks.AddRangeAsync(syncBlocks
            //    .Where(x => newBlocks.All(y => y.Hash != x.Hash))
            //    .Select(x => _mapper.Map<OrphanBlock>(x)));

            //await _nexusDb.SaveChangesAsync();
        }
    }
}
