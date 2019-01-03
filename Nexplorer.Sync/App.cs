using System;
using System.Threading.Tasks;
using System.Timers;
using Hangfire;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Data.Query;
using Nexplorer.Sync.Hangfire;
using Nexplorer.Sync.Hangfire.Catchup;

namespace Nexplorer.Sync
{
    public class App
    {
        private readonly BackgroundJobServer _hangfire;
        private readonly NexusQuery _nexusQuery;
        private readonly BlockSyncCatchup _blockCatchup;
        private readonly AddressAggregateCatchup _addressCatchup;
        private readonly BlockCacheJob _blockCacheJob;
        private readonly BlockCacheService _blockCache;
        private readonly RedisCommand _redisCommand;

        public App(NexusQuery nexusQuery, BlockSyncCatchup blockCatchup, AddressAggregateCatchup addressCatchup,
            BlockCacheJob blockCacheJob, BlockCacheService blockCache, RedisCommand redisCommand)
        {
            _nexusQuery = nexusQuery;
            _blockCatchup = blockCatchup;
            _addressCatchup = addressCatchup;
            _blockCacheJob = blockCacheJob;
            _blockCache = blockCache;
            _redisCommand = redisCommand;
        }

        public async Task StartAsync()
        {
            await _blockCatchup.CatchupAsync();
            await _addressCatchup.CatchupAsync();

            await _redisCommand.SetAsync(Settings.Redis.NodeVersion, (await _nexusQuery.GetInfoAsync()).Version);

            await _blockCacheJob.CreateAsync();
            await _blockCache.GetBlocksAsync();

            var server = new BackgroundJobServer();
            
            BackgroundJob.Schedule(() => StartJobs(), TimeSpan.FromSeconds(10));
        }

        [DisableConcurrentExecution(10)]
        public void StartJobs()
        {
            BackgroundJob.Schedule<BlockSyncJob>(x => x.SyncLatestAsync(), BlockSyncJob.JobInterval);
            BackgroundJob.Schedule<BlockCacheCleanupJob>(x => x.CleanUpAsync(), BlockCacheCleanupJob.JobInterval);
            BackgroundJob.Schedule<TrustAddressCacheJob>(x => x.UpdateTrustAddressesAsync(), TrustAddressCacheJob.JobInterval);
            BackgroundJob.Schedule<NexusAddressCacheJob>(x => x.CacheNexusAddressesAsync(), NexusAddressCacheJob.JobInterval);
            BackgroundJob.Schedule<AddressStatsJob>(x => x.UpdateStatsAsync(), AddressStatsJob.JobInterval);
            BackgroundJob.Schedule<ExchangeSyncJob>(x => x.SyncAsync(), ExchangeSyncJob.JobInterval);
            BackgroundJob.Enqueue<BlockScanJob>(x => x.ScanAsync(null));

            BackgroundJob.Enqueue<StatsJob>(x => x.UpdateTimestamp());
            BackgroundJob.Enqueue<StatsJob>(x => x.PublishMiningStatsAsync());
            BackgroundJob.Enqueue<StatsJob>(x => x.UpdateMiningHistoricalAsync());
        }
    }
}