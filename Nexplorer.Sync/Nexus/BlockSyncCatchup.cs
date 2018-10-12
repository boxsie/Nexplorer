using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Cache.Block;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Data.Command;
using Nexplorer.Data.Context;
using Nexplorer.Data.Map;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Sync.Nexus
{
    public class AddressAggregateCatchup
    {
        private readonly NexusDb _nexusDb;
        private readonly ILogger<AddressAggregateCatchup> _logger;
        private readonly AddressAggregateUpdateCommand _addressAggregateUpdate;

        public AddressAggregateCatchup(ILogger<AddressAggregateCatchup> logger, NexusDb nexusDb,
            AddressAggregateUpdateCommand addressAggregateUpdate)
        {
            _logger = logger;
            _addressAggregateUpdate = addressAggregateUpdate;
            _nexusDb = nexusDb;
        }

        public async Task Catchup()
        {
            var lastBlockHeight = await GetLastBlockHeight();

            var dbHeight = await _nexusDb.Blocks
                .OrderBy(x => x.Height)
                .Select(x => x.Height)
                .LastOrDefaultAsync();

            while (lastBlockHeight < dbHeight)
            {
                var nextBlockHeight = lastBlockHeight + 1;

                var updateCount = 0;

                var bulkSaveCount = Settings.App.BulkSaveCount;

                var lastHeight = dbHeight - nextBlockHeight > bulkSaveCount
                    ? nextBlockHeight + bulkSaveCount
                    : dbHeight;

                _logger.LogInformation($"Adding address aggregate data from block {nextBlockHeight} -> {lastHeight}");

                for (var i = nextBlockHeight; i < nextBlockHeight + bulkSaveCount; i++)
                {
                    var height = i;

                    if (height > dbHeight)
                        break;

                    var nextBlock = await _nexusDb.Blocks
                        .Include(x => x.Transactions)
                        .ThenInclude(x => x.Inputs)
                        .ThenInclude(x => x.Address)
                        .Include(x => x.Transactions)
                        .ThenInclude(x => x.Outputs)
                        .ThenInclude(x => x.Address)
                        .FirstOrDefaultAsync(x => x.Height == height);

                    if (nextBlock == null)
                        break;

                    foreach (var transaction in nextBlock.Transactions)
                    {
                        foreach (var transactionInput in transaction.Inputs)
                            await _addressAggregateUpdate.UpdateAsync(_nexusDb, transactionInput.Address.AddressId,
                                TransactionType.Input, transactionInput.Amount, nextBlock);

                        foreach (var transactionOutput in transaction.Outputs)
                            await _addressAggregateUpdate.UpdateAsync(_nexusDb, transactionOutput.Address.AddressId,
                                TransactionType.Output, transactionOutput.Amount, nextBlock);
                    }

                    updateCount++;
                }

                await _nexusDb.SaveChangesAsync();

                _addressAggregateUpdate.Reset();

                _logger.LogInformation($"{updateCount} address aggregates saved");

                lastBlockHeight = await GetLastBlockHeight();
            }
        }

        private async Task<int> GetLastBlockHeight()
        {
            return await _nexusDb.AddressAggregates.AnyAsync()
                ? await _nexusDb.AddressAggregates.MaxAsync(x => x.LastBlock.Height)
                : 0;
        }
    }

    public class BlockSyncCatchup
    {
        private readonly NexusQuery _nexusQuery;
        private readonly NexusDb _nexusDb;
        private readonly BlockQuery _blockQuery;
        private readonly BlockMapper _blockAdd;
        private readonly BlockCacheBuild _blockCacheBuild;
        private readonly ILogger<BlockSyncCatchup> _logger;
        private readonly RedisCommand _redisCommand;

        public BlockSyncCatchup(NexusQuery nexusQuery, NexusDb nexusDb, BlockQuery blockQuery, BlockMapper blockAdd, 
            BlockCacheBuild blockCacheBuild, ILogger<BlockSyncCatchup> logger, RedisCommand redisCommand)
        {
            _nexusQuery = nexusQuery;
            _nexusDb = nexusDb;
            _blockQuery = blockQuery;
            _blockAdd = blockAdd;
            _blockCacheBuild = blockCacheBuild;
            _logger = logger;
            _redisCommand = redisCommand;
        }

        public async Task Catchup()
        {
            await _redisCommand.SetAsync(Settings.Redis.NodeVersion, (await _nexusQuery.GetInfoAsync()).Version);

            var syncedHeight = await _blockQuery.GetLastSyncedHeightAsync();
            var nexusHeight = await _nexusQuery.GetBlockchainHeightAsync();

            while (nexusHeight == 0)
            {
                _logger.LogWarning("Nexus node is unavailible at this time...");

                Thread.Sleep(10000);

                nexusHeight = await _nexusQuery.GetBlockchainHeightAsync();
            }

            while(nexusHeight < syncedHeight)
            {
                _logger.LogWarning($"Nexus database is {syncedHeight - nexusHeight} blocks behind. Waiting for Nexus to catchup...");

                Thread.Sleep(10000);

                nexusHeight = await _nexusQuery.GetBlockchainHeightAsync();
            }

            var stopwatch = new Stopwatch();
            var totalSeconds = 0;
            var iterationCount = 0;

            while (syncedHeight + Settings.App.BlockCacheCount < nexusHeight)
            {
                var syncDelta = nexusHeight - syncedHeight;

                if (stopwatch.IsRunning)
                {
                    iterationCount++;
                    totalSeconds += stopwatch.Elapsed.Seconds;

                    var avgSeconds = totalSeconds / iterationCount;
                    var estRemainingIterations = syncDelta / Settings.App.BulkSaveCount;

                    var remainingTime = TimeSpan.FromSeconds(estRemainingIterations * avgSeconds);

                    _logger.LogInformation($"Save complete. Iteration took { stopwatch.Elapsed.Seconds } seconds");
                    _logger.LogInformation($"Estimated remaining sync time: { remainingTime }");

                    stopwatch.Reset();
                }

                stopwatch.Start();

                _logger.LogInformation($"Sync is { syncDelta } blocks behind Nexus");

                var dbBlocks = new List<BlockDto>();

                var saveCount = Settings.App.BulkSaveCount;

                if (syncDelta - Settings.App.BulkSaveCount < Settings.App.BlockCacheCount)
                    saveCount = Settings.App.BulkSaveCount - (Settings.App.BlockCacheCount - (syncDelta - Settings.App.BulkSaveCount));

                var blockDto = await _nexusQuery.GetBlockAsync(syncedHeight + 1, true);
                
                _logger.LogInformation($"Syncing blocks { blockDto.Height } -> { (blockDto.Height + saveCount) - 1 }...");

                for (var i = 0; i < saveCount; i++)
                {
                    if (blockDto == null)
                        break;

                    if (i % 5 == 0)
                        _logger.LogInformation($"{i} out of {saveCount} synced");

                    dbBlocks.Add(blockDto);

                    blockDto = await _nexusQuery.GetBlockAsync(blockDto.Height + 1, true);
                }
                
                _logger.LogInformation("Sync complete. Performing sync save...");

                var blocks = await _blockAdd.MapBlocksAsync(dbBlocks);

                await _nexusDb.AddRangeAsync(blocks);
                await _nexusDb.SaveChangesAsync();
                
                nexusHeight = await _nexusQuery.GetBlockchainHeightAsync();
                syncedHeight = await _blockQuery.GetLastSyncedHeightAsync();
            }

            _logger.LogInformation("Database sync is up to date");

            await _blockCacheBuild.BuildAsync(syncedHeight + 1);
        }
    }
}
