//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using Nexplorer.Config;
//using Nexplorer.Core;
//using Nexplorer.Data.Query;
//using Nexplorer.Domain.Dtos;

//namespace Nexplorer.Data.Cache.Block
//{
//    public class BlockStream
//    {
//        public bool AllowProgressUpdate { get; set; }

//        private readonly NexusQuery _nexusQuery;
//        private readonly RedisCommand _redisCommand;

//        public BlockStream(NexusQuery nexusQuery, RedisCommand redisCommand)
//        {
//            _nexusQuery = nexusQuery;
//            _redisCommand = redisCommand;

//            AllowProgressUpdate = true;
//        }

//        public void StartStreamingNexusBlocks(int startingHeight, CancellationToken _cancel)
//        {
//#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
//            Task.Run(() => BlockScan(startingHeight, _cancel), _cancel);
//#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
//        }

//        public Task<int> GetCacheHeight()
//        {
//            return _redisCommand.GetAsync<int>(Settings.Redis.BlockSyncStreamCacheHeight);
//        }

//        public async Task<List<BlockDto>> GetCacheBlocksAsync(int startHeight, int? count = 1)
//        {
//            var cacheBlocks = new List<BlockDto>();

//            var end = count.HasValue 
//                ? startHeight + count 
//                : await GetCacheHeight();

//            for (var i = startHeight; i < end; i++)
//            {
//                var block = await _redisCommand.GetAsync<BlockDto>(CreateStreamKey(i));

//                if (block == null)
//                    break;

//                cacheBlocks.Add(block);
//            }

//            return cacheBlocks;
//        }

//        private async Task DeleteCacheBlocks(int startHeight, int count)
//        {
//            var startingHeight = await _redisCommand.GetAsync<int>(Settings.Redis.BlockSyncStreamCacheStart);

//            await _redisCommand.SetAsync<int>(Settings.Redis.BlockSyncStreamCacheStart, startingHeight + count);

//            for (var i = startHeight; i <= count; i++)
//                await _redisCommand.DeleteAsync(CreateStreamKey(i));
//        }

//        private async Task BlockScan(int startingHeight, CancellationToken cancel)
//        {
//            await _redisCommand.SetAsync<int>(Settings.Redis.BlockSyncStreamCacheStart, startingHeight);

//            var nexusHeight = await _nexusQuery.GetBlockchainHeightAsync();

//            var cacheStart = startingHeight;
//            var height = startingHeight;

//            while (true)
//            {
//                var blockDto = await _nexusQuery.GetBlockAsync(height, true);

//                if (blockDto == null)
//                {
//                    await Task.Delay(1, cancel);
//                    continue;
//                }

//                await _redisCommand.SetAsync(CreateStreamKey(blockDto.Height), blockDto);
//                await _redisCommand.SetAsync(Settings.Redis.BlockSyncStreamCacheHeight, blockDto.Height);
//                await _redisCommand.PublishAsync(Settings.Redis.BlockSyncStreamCacheHeight, blockDto.Height);

//                var cacheSize = height - cacheStart;

//                if (cacheSize > Settings.App.StreamCacheCount)
//                    await DeleteCacheBlocks(cacheStart, cacheSize - Settings.App.StreamCacheCount);

//                if (height % 1000 == 0)
//                    nexusHeight = await _nexusQuery.GetBlockchainHeightAsync();

//                if (AllowProgressUpdate)
//                    Console.Write(string.Format("\rStreaming Nexus blocks... {0} {1:N4}% ({2:N0}/{3:N0}) | {4} blocks in cache",
//                        LogProgress(blockDto.Height, nexusHeight, out var streamPct), streamPct, blockDto.Height, nexusHeight, cacheSize));

//                height++;
//            }
//        }

//        private static string CreateStreamKey(int blockHeight)
//        {
//            return $"{Settings.Redis.BlockSyncStreamCache}:{blockHeight}";
//        }

//        private static string LogProgress(int i, int total, out double pct)
//        {
//            pct = ((double)i / total) * 100;

//            var progress = Math.Floor((double)pct / 5);
//            var bar = "";

//            for (var o = 0; o < 20; o++)
//            {
//                bar += progress > o
//                    ? '#'
//                    : ' ';
//            }

//            return $"[{bar}]";
//        }
//    }
//}