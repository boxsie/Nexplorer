using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Context;
using Nexplorer.Data.Map;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Infrastructure.Bittrex;

namespace Nexplorer.Sync.Hangfire
{
    public class ExchangeSyncJob
    {
        public static readonly TimeSpan JobInterval = TimeSpan.FromSeconds(5);

        private readonly BittrexClient _bittrexClient;
        private readonly NexusDb _nexusDb;
        private readonly ILogger<ExchangeSyncJob> _logger;
        private readonly RedisCommand _redis;

        private const string Market = "BTC-NXS";
        private const string UsdMarket = "USDT-BTC";

        private static int _runCount;
        private const int DbSaveRunInterval = 5;

        public ExchangeSyncJob(BittrexClient bittrexClient, NexusDb nexusDb, ILogger<ExchangeSyncJob> logger, RedisCommand redis)
        {
            _bittrexClient = bittrexClient;
            _nexusDb = nexusDb;
            _logger = logger;
            _redis = redis;
        }

        [DisableConcurrentExecution(10)]
        public async Task SyncAsync()
        {
            var nxsSummary = await _bittrexClient.GetMarketSummaryAsync(Market);
            var btcTicket = await _bittrexClient.GetTickerAsync(UsdMarket);

            if (btcTicket != null)
                await _redis.SetAsync(Settings.Redis.BittrexLastUsdBtcPrice, btcTicket.Last);

            if (nxsSummary == null)
            {
                _logger.LogInformation("Bittrex data not availible");
                return;
            }

            await _redis.SetAsync(Settings.Redis.BittrexLastBtcNxsPrice, nxsSummary.Last);

            var summary = nxsSummary.ToBittrexSummary();

            Interlocked.Increment(ref _runCount);
            
            if (_runCount == DbSaveRunInterval)
            {
                await _nexusDb.BittrexSummaries.AddAsync(summary);

                await _nexusDb.SaveChangesAsync();
                
                Interlocked.Exchange(ref _runCount, 0);

                _logger.LogInformation("Latest Bittrex data saved to DB");
            }

            var summaryDto = new BittrexSummaryDto(summary);

            await _redis.PublishAsync(Settings.Redis.BittrexSummaryPubSub, summaryDto);
            await _redis.SetAsync(Settings.Redis.BittrexSummaryPubSub, summaryDto);

            BackgroundJob.Schedule<ExchangeSyncJob>(x => x.SyncAsync(), JobInterval);
        }
    }

    public class BlockCacheCleanupJob
    {
        public static readonly TimeSpan JobInterval = TimeSpan.FromMinutes(1);

        private readonly ILogger<BlockCacheCleanupJob> _logger;
        private readonly BlockQuery _blockQuery;
        private readonly RedisCommand _redisCommand;

        public BlockCacheCleanupJob(ILogger<BlockCacheCleanupJob> logger, BlockQuery blockQuery, RedisCommand redisCommand)
        {
            _logger = logger;
            _blockQuery = blockQuery;
            _redisCommand = redisCommand;
        }

        public async Task CleanUpAsync()
        {
            var heightToDelete = await _blockQuery.GetLastSyncedHeightAsync();
            var cacheBlock = await GetCacheBlock(heightToDelete);

            while (cacheBlock != null)
            {
                _logger.LogInformation($"Deleting block {heightToDelete} from the cache");

                await _redisCommand.DeleteAsync(Settings.Redis.BuildCachedBlockKey(heightToDelete));

                heightToDelete--;
                cacheBlock = await GetCacheBlock(heightToDelete);
            }

            BackgroundJob.Schedule<BlockCacheCleanupJob>(x => x.CleanUpAsync(), JobInterval);
        }

        private Task<BlockDto> GetCacheBlock(int height)
        {
            return _redisCommand.GetAsync<BlockDto>(Settings.Redis.BuildCachedBlockKey(height));
        }
    }
}