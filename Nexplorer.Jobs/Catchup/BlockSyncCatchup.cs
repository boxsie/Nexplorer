using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Command;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Jobs.Catchup
{
    public class BlockSyncCatchup
    {
        private readonly NexusQuery _nexusQuery;
        private readonly IServiceProvider _serviceProvider;
        private readonly BlockQuery _blockQuery;
        private readonly ILogger<BlockSyncCatchup> _logger;
        private readonly RedisCommand _redisCommand;
        private readonly BlockInsertCommand _blockInsert;
        private readonly BlockCacheCommand _cacheCommand;
        private readonly CancellationTokenSource _cancelBlockStream;

        private Stopwatch _stopwatch;
        private double _totalSeconds;
        private int _iterationCount;
        private bool _allowProgressUpdate;
        private int _streamCount;
        private int _nexusHeight;

        public BlockSyncCatchup(NexusQuery nexusQuery, IServiceProvider serviceProvider, BlockQuery blockQuery, 
            ILogger<BlockSyncCatchup> logger, RedisCommand redisCommand, BlockInsertCommand blockInsert, BlockCacheCommand cacheCommand)
        {
            _nexusQuery = nexusQuery;
            _serviceProvider = serviceProvider;
            _blockQuery = blockQuery;
            _logger = logger;
            _redisCommand = redisCommand;
            _blockInsert = blockInsert;
            _cacheCommand = cacheCommand;
            _cancelBlockStream = new CancellationTokenSource();
        }

        public async Task CatchupAsync()
        {
            var syncedHeight = await _blockQuery.GetLastHeightAsync();
            _nexusHeight = await _nexusQuery.GetBlockchainHeightAsync();

            while (_nexusHeight == 0)
            {
                _logger.LogWarning("Nexus node is unavailible at this time...");

                Thread.Sleep(10000);

                _nexusHeight = await _nexusQuery.GetBlockchainHeightAsync();
            }

            while(_nexusHeight < syncedHeight)
            {
                _logger.LogWarning($"Nexus database is {syncedHeight - _nexusHeight} blocks behind. Waiting for Nexus to catchup...");

                Thread.Sleep(10000);

                _nexusHeight = await _nexusQuery.GetBlockchainHeightAsync();
            }

            await StartStreamingNexusBlocks(syncedHeight + 1);

            _stopwatch = new Stopwatch();
            _totalSeconds = 0;
            _iterationCount = 0;

            while (syncedHeight < _nexusHeight)
            {
                var syncDelta = _nexusHeight - syncedHeight;

                Console.WriteLine($"\nSync is {syncDelta:N0} blocks behind Nexus");
                
                var saveCount = syncDelta < Settings.App.BulkSaveCount ? syncDelta : Settings.App.BulkSaveCount;
                
                _stopwatch.Start();

                await SyncBlocks(syncedHeight, saveCount);

                _stopwatch.Stop();

                _nexusHeight = await _nexusQuery.GetBlockchainHeightAsync();
                syncedHeight = await _blockQuery.GetLastHeightAsync();

                LogTimeTaken(syncDelta, _stopwatch.Elapsed);

                _stopwatch.Reset();

                _allowProgressUpdate = true;
            }

            Console.WriteLine();
            _logger.LogInformation("Stopping Nexus block stream");
            _cancelBlockStream.Cancel();

            _logger.LogInformation("Database sync is up to date");
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

            Console.WriteLine($"\nSyncing blocks from height {(syncedHeight + 1):N0} - {(syncedHeight + saveCount):N0}...");

            for (var i = syncedHeight + 1; i <= syncedHeight + saveCount; i++)
                nexusBlocks.Add(await _redisCommand.GetAsync<BlockDto>(CreateStreamKey(i)));

            Console.WriteLine("Sync complete. Performing sync save...");

            await _blockInsert.InsertBlocksAsync(nexusBlocks);

            foreach (var nexusBlock in nexusBlocks)
                await _redisCommand.DeleteAsync(CreateStreamKey(nexusBlock.Height));

            _streamCount -= nexusBlocks.Count;
        }

        private async Task StartStreamingNexusBlocks(int startingHeight)
        {
            _allowProgressUpdate = true;

            var blockDto = await _nexusQuery.GetBlockAsync(startingHeight, true);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                while (blockDto != null)
                {
                    _cancelBlockStream.Token.ThrowIfCancellationRequested();

                    await _redisCommand.SetAsync(CreateStreamKey(blockDto.Height), blockDto);
                    await _redisCommand.SetAsync(Settings.Redis.BlockSyncStreamCacheHeight, blockDto.Height);
                    
                    if (_allowProgressUpdate)
                        Console.Write($"\rStreaming Nexus blocks... {LogProgress(blockDto.Height, _nexusHeight, out var streamPct)} {streamPct:N4}% ({blockDto.Height:N0}/{_nexusHeight:N0}) | Stream is {_streamCount} blocks ahead        ");

                    _streamCount++;

                    blockDto = await _nexusQuery.GetBlockAsync(blockDto.Height + 1, true);
                }
            }, _cancelBlockStream.Token);
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

            Console.WriteLine($"\nSave complete. Iteration took { timeTaken }");
            Console.WriteLine($"Estimated remaining sync time: { remainingTime }");
        }

        private static string LogProgress(int i, int total, out double pct)
        {
            pct = ((double)i / total) * 100;

            var progress = Math.Floor((double)pct / 5);
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
