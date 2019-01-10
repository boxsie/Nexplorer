using System;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Data.Services;
using Nexplorer.Jobs;
using Nexplorer.Jobs.Catchup;
using Nexplorer.Jobs.Service;

namespace Nexplorer.Sync
{
    public class App
    {
        private readonly NexusQuery _nexusQuery;
        private readonly BlockSyncCatchup _blockCatchup;
        private readonly AddressAggregateCatchup _addressCatchup;
        private readonly CacheService _cache;
        private readonly RedisCommand _redisCommand;

        public App(NexusQuery nexusQuery, BlockSyncCatchup blockCatchup, AddressAggregateCatchup addressCatchup, CacheService cache, RedisCommand redisCommand)
        {
            _nexusQuery = nexusQuery;
            _blockCatchup = blockCatchup;
            _addressCatchup = addressCatchup;
            _cache = cache;
            _redisCommand = redisCommand;
        }

        public async Task StartAsync()
        {
            await _blockCatchup.CatchupAsync();
            await _addressCatchup.CatchupAsync();

            await _redisCommand.SetAsync(Settings.Redis.NodeVersion, (await _nexusQuery.GetInfoAsync()).Version);

            await _cache.CreateAsync();

            await Task.WhenAll(
                JobService.StartJob(typeof(BlockScanJob)),
                JobService.StartJob(typeof(TimestampSyncJob)),
                JobService.StartJob(typeof(ExchangeSyncJob)),
                JobService.StartJob(typeof(MiningInfoSyncJob)),
                JobService.StartJob(typeof(BlockSyncJob), 20),
                JobService.StartJob(typeof(NexusAddressCacheJob), 20),
                JobService.StartJob(typeof(TrustAddressCacheJob), 40),
                JobService.StartJob(typeof(CacheCleanupJob), 40),
                JobService.StartJob(typeof(MiningStatsJob), 40),
                JobService.StartJob(typeof(AddressStatsJob), 60));
        }
    }
}