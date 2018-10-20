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
        private readonly BlockCacheBuild _blockCacheBuild;
        private readonly ILogger<BlockSyncCatchup> _logger;
        private readonly RedisCommand _redisCommand;

        private Stopwatch _stopwatch;
        private double _totalSeconds;
        private int _iterationCount;
        private bool _allowProgressUpdate;

        public BlockSyncCatchup(NexusQuery nexusQuery, IServiceProvider serviceProvider, BlockQuery blockQuery,
            BlockCacheBuild blockCacheBuild, ILogger<BlockSyncCatchup> logger, RedisCommand redisCommand)
        {
            _nexusQuery = nexusQuery;
            _serviceProvider = serviceProvider;
            _blockQuery = blockQuery;
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

            await StartStreamingNexusBlocks(syncedHeight + 1, nexusHeight, Settings.App.BulkSaveCount);

            _stopwatch = new Stopwatch();
            _totalSeconds = 0;
            _iterationCount = 0;

            while (syncedHeight + Settings.App.BlockCacheCount < nexusHeight)
            {
                var syncDelta = nexusHeight - syncedHeight;

                Console.WriteLine($"Sync is {syncDelta:N0} blocks behind Nexus");
                
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

                _allowProgressUpdate = true;
            }

            _logger.LogInformation("Database sync is up to date");

            await _blockCacheBuild.BuildAsync(syncedHeight + 1);
        }

        private async Task SyncBlocks(int syncedHeight, int saveCount)
        {
            var streamCount = await _redisCommand.GetAsync<int>(Settings.Redis.BlockSyncStreamCacheHeight);

            while (streamCount < syncedHeight + saveCount)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                streamCount = await _redisCommand.GetAsync<int>(Settings.Redis.BlockSyncStreamCacheHeight);
            }

            _allowProgressUpdate = false;

            var nexusBlocks = new List<BlockDto>();

            Console.WriteLine($"\nSyncing blocks from height {(syncedHeight + 1):N0} - {(syncedHeight + 1 + saveCount):N0}...");

            for (var i = syncedHeight + 1; i <= syncedHeight + saveCount; i++)
                nexusBlocks.Add(await _redisCommand.GetAsync<BlockDto>(CreateStreamKey(i)));

            Console.WriteLine("Sync complete. Performing sync save...");

            await nexusBlocks.InsertBlocksAsync();

            foreach (var nexusBlock in nexusBlocks)
                await _redisCommand.DeleteAsync(CreateStreamKey(nexusBlock.Height));
        }

        private async Task StartStreamingNexusBlocks(int startingHeight, int nexusHeight, int saveCount)
        {
            _allowProgressUpdate = true;

            var blockDto = await _nexusQuery.GetBlockAsync(startingHeight, true);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                var countUntilSave = saveCount;

                while (blockDto != null)
                {
                    await _redisCommand.SetAsync(CreateStreamKey(blockDto.Height), blockDto);
                    await _redisCommand.SetAsync(Settings.Redis.BlockSyncStreamCacheHeight, blockDto.Height);
                    
                    if (_allowProgressUpdate)
                        Console.Write($"\rStreaming Nexus blocks... {LogProgress(blockDto.Height, nexusHeight, out var streamPct)} {streamPct:N4}% ({blockDto.Height:N0}/{nexusHeight:N0}) | Stream sync in {countUntilSave} blocks        ");

                    countUntilSave--;

                    if (countUntilSave < 0)
                        countUntilSave = saveCount;

                    blockDto = await _nexusQuery.GetBlockAsync(blockDto.Height + 1, true);
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        }

        private string CreateStreamKey(int blockHeight)
        {
            return $"{Settings.Redis.BlockSyncStreamCache}:{blockHeight}";
        }

        private void LogTimeTaken(int syncDelta, TimeSpan timeTaken)
        {
            _iterationCount++;
            
            _totalSeconds += timeTaken.TotalSeconds;

            var avgSeconds = _totalSeconds / _iterationCount;
            var estRemainingIterations = syncDelta / Settings.App.BulkSaveCount;

            var remainingTime = TimeSpan.FromSeconds(estRemainingIterations * avgSeconds);

            Console.WriteLine($"Save complete. Iteration took { timeTaken }");
            Console.WriteLine($"Estimated remaining sync time: { remainingTime }");
        }

        private static string LogProgress(int i, int total, out double pct)
        {
            pct = ((double)i / total) * 100;

            var progress = Math.Floor((double)pct / 2);
            var bar = "";

            for (var o = 0; o < 20; o++)
            {
                bar += progress > o
                    ? '#'
                    : ' ';
            }

            return $"[{bar}]";
        }
    }
}
