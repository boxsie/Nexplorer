using System;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Command;
using Nexplorer.Data.Query;
using Nexplorer.Data.Services;
using Nexplorer.Jobs;
using Nexplorer.Jobs.Catchup;
using Nexplorer.Jobs.Service;

namespace Nexplorer.Sync
{
    public class App
    {
        private readonly ILogger<App> _logger;
        private readonly NexusQuery _nexusQuery;
        private readonly BlockSyncCatchup _blockCatchup;
        private readonly AddressAggregateCatchup _addressCatchup;
        private readonly BlockCacheCommand _cacheCommand;
        private readonly RedisCommand _redisCommand;

        public App(ILogger<App> logger, NexusQuery nexusQuery, BlockSyncCatchup blockCatchup, 
            AddressAggregateCatchup addressCatchup, BlockCacheCommand cacheCommand, RedisCommand redisCommand)
        {
            _logger = logger;
            _nexusQuery = nexusQuery;
            _blockCatchup = blockCatchup;
            _addressCatchup = addressCatchup;
            _cacheCommand = cacheCommand;
            _redisCommand = redisCommand;
        }

        public async Task StartAsync()
        {
            if (await EnsureNodeIsReady())
            {
                await _blockCatchup.CatchupAsync();
                await _addressCatchup.CatchupAsync();
                await _cacheCommand.BuildAsync();

                await Task.WhenAll(
                    JobService.StartJob(typeof(BlockScanJob)),
                    JobService.StartJob(typeof(TimestampSyncJob)),
                    JobService.StartJob(typeof(ExchangeSyncJob)),
                    JobService.StartJob(typeof(MiningInfoSyncJob)),
                    JobService.StartJob(typeof(BlockSyncJob), 20),
                    JobService.StartJob(typeof(NexusAddressCacheJob), 20),
                    JobService.StartJob(typeof(TrustAddressCacheJob), 40),
                    JobService.StartJob(typeof(MiningStatsJob), 40),
                    JobService.StartJob(typeof(AddressStatsJob), 60));
            }
        }

        private async Task<bool> EnsureNodeIsReady()
        {
            var nxsVersion = await _nexusQuery.GetInfoAsync();

            while (nxsVersion == null)
            {
                _logger.LogCritical("Nexus node is not available at this time.....");

                nxsVersion = await _nexusQuery.GetInfoAsync();

                await Task.Delay(TimeSpan.FromSeconds(10));
            }

            _logger.LogInformation($"Nexus node connected on version {nxsVersion.Version}");

            await _redisCommand.SetAsync(Settings.Redis.NodeVersion, nxsVersion.Version);

            return true;
        }
    }
}