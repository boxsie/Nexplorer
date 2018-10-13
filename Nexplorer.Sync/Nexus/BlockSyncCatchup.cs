using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using Nexplorer.Domain.Entity.Blockchain;
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
                ? await _nexusDb.AddressAggregates.MaxAsync(x => x.LastBlockHeight)
                : 0;
        }
    }

    public class BlockSyncCatchup
    {
        private readonly NexusQuery _nexusQuery;
        private readonly IServiceProvider _serviceProvider;
        private readonly BlockQuery _blockQuery;
        private readonly BlockMapper _blockAdd;
        private readonly BlockCacheBuild _blockCacheBuild;
        private readonly ILogger<BlockSyncCatchup> _logger;
        private readonly RedisCommand _redisCommand;

        private Stopwatch _stopwatch;
        private double _totalSeconds;
        private int _iterationCount;

        public BlockSyncCatchup(NexusQuery nexusQuery, IServiceProvider serviceProvider, BlockQuery blockQuery, BlockMapper blockAdd, 
            BlockCacheBuild blockCacheBuild, ILogger<BlockSyncCatchup> logger, RedisCommand redisCommand)
        {
            _nexusQuery = nexusQuery;
            _serviceProvider = serviceProvider;
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

            _stopwatch = new Stopwatch();
            _totalSeconds = 0;
            _iterationCount = 0;

            while (syncedHeight + Settings.App.BlockCacheCount < nexusHeight)
            {
                var syncDelta = nexusHeight - syncedHeight;
                
                _logger.LogInformation($"Sync is { syncDelta } blocks behind Nexus");
                
                var saveCount = Settings.App.BulkSaveCount;

                if (syncDelta - Settings.App.BulkSaveCount < Settings.App.BlockCacheCount)
                    saveCount = Settings.App.BulkSaveCount - (Settings.App.BlockCacheCount - (syncDelta - Settings.App.BulkSaveCount));
                
                _stopwatch.Start();

                await SyncBlocks(syncedHeight, saveCount);

                _stopwatch.Stop();

                nexusHeight = await _nexusQuery.GetBlockchainHeightAsync();
                syncedHeight = await _blockQuery.GetLastSyncedHeightAsync();

                LogTimeTaken(syncDelta, _stopwatch.Elapsed);

                _stopwatch.Reset();
            }

            _logger.LogInformation("Database sync is up to date");

            await _blockCacheBuild.BuildAsync(syncedHeight + 1);
        }

        private async Task SyncBlocks(int syncedHeight, int saveCount)
        {
            var nexusBlocks = await GetBlocksFromNexus(syncedHeight, saveCount);
            await SaveBlocksToDb(nexusBlocks);
        }

        private async Task<List<BlockDto>> GetBlocksFromNexus(int syncedHeight, int saveCount)
        {
            var dbBlocks = new List<BlockDto>();

            var blockDto = await _nexusQuery.GetBlockAsync(syncedHeight + 1, true);

            _logger.LogInformation($"Syncing blocks { blockDto.Height } -> { (blockDto.Height + saveCount) - 1 }...");

            for (var i = 0; i < saveCount; i++)
            {
                if (blockDto == null)
                    break;

                dbBlocks.Add(blockDto);

                blockDto = await _nexusQuery.GetBlockAsync(blockDto.Height + 1, true);

                LogProgress(i, saveCount);
            }

            Console.Write($"\r");
            return dbBlocks;
        }

        private async Task SaveBlocksToDb(List<BlockDto> nexusBlocks)
        {
            _logger.LogInformation("Sync complete. Performing sync save...");

            using (var context = (NexusDb)_serviceProvider.GetService(typeof(NexusDb)))
            {
                var blocks = await _blockAdd.MapBlocksAsync(context, nexusBlocks);

                await context.AddRangeAsync(blocks);
                await context.SaveChangesAsync();
            }
        }

        private void LogTimeTaken(int syncDelta, TimeSpan timeTaken)
        {
            _iterationCount++;
            
            _totalSeconds += timeTaken.TotalSeconds;

            var avgSeconds = _totalSeconds / _iterationCount;
            var estRemainingIterations = syncDelta / Settings.App.BulkSaveCount;

            var remainingTime = TimeSpan.FromSeconds(estRemainingIterations * avgSeconds);

            _logger.LogInformation($"Save complete. Iteration took { timeTaken }");
            _logger.LogInformation($"Estimated remaining sync time: { remainingTime }");
        }

        private static void LogProgress(int i, int saveCount)
        {
            var syncedPct = ((double)i / saveCount) * 100;
            var progress = Math.Floor((double)syncedPct / 2);
            var bar = "";

            for (var o = 0; o < 50; o++)
            {
                bar += progress > o
                    ? '#'
                    : ' ';
            }

            Console.Write($"\rSyncing {saveCount} blocks... [{bar}] {Math.Floor(syncedPct)}% ");
        }
    }
}
