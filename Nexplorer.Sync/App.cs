using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Data.Cache.Block;
using Nexplorer.Data.Query;
using Nexplorer.Sync.Core;
using Nexplorer.Sync.Jobs;
using Nexplorer.Sync.Nexus;
using Quartz;
using Quartz.Impl;

namespace Nexplorer.Sync
{
    public class App
    {
        private readonly ILogger<App> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly BlockSyncCatchup _blockCatchup;
        private readonly AddressAggregateCatchup _addressAggregateCatchup;
        private readonly BlockCacheBuild _blockCacheBuild;

        public App(ILogger<App> logger, IServiceProvider serviceProvider, BlockSyncCatchup blockCatchup, 
            AddressAggregateCatchup addressAggregateCatchup, BlockCacheBuild blockCacheBuild)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _blockCatchup = blockCatchup;
            _addressAggregateCatchup = addressAggregateCatchup;
            _blockCacheBuild = blockCacheBuild;
        }

        public async Task Run()
        {
            await _blockCatchup.Catchup();
            await _addressAggregateCatchup.Catchup();
            await _blockCacheBuild.BuildAsync();

            await StartJobs();
        }

        private async Task StartJobs()
        {
            var schedFact = new StdSchedulerFactory();
            var jobFact = new JobFactory(_serviceProvider);

            var scheduler = await schedFact.GetScheduler();

            scheduler.JobFactory = jobFact;

            await scheduler.Start();

            var jobs = new Dictionary<string, SyncJob>();

            jobs.Add(nameof(BlockScanJob), (SyncJob)_serviceProvider.GetService(typeof(BlockScanJob)));
            jobs.Add(nameof(BlockSyncJob), (SyncJob)_serviceProvider.GetService(typeof(BlockSyncJob)));
            jobs.Add(nameof(AddressCacheJob), (SyncJob)_serviceProvider.GetService(typeof(AddressCacheJob)));
            jobs.Add(nameof(AddressStatsJob), (SyncJob)_serviceProvider.GetService(typeof(AddressStatsJob)));
            jobs.Add(nameof(MiningStatsJob), (SyncJob)_serviceProvider.GetService(typeof(MiningStatsJob)));

            foreach (var jt in jobs)
            {
                var jobInstance = jt.Value;

                var job = JobBuilder.Create(jt.Value.GetType())
                    .WithIdentity($"{jt.Key}Job")
                    .Build();

                var offsetSecs = 0;

                switch (jt.Key)
                {
                    case nameof(BlockScanJob):
                        offsetSecs = 120;
                        break;
                    case nameof(BlockSyncJob):
                        offsetSecs = 120;
                        break;
                    case nameof(AddressCacheJob):
                        offsetSecs = 0;
                        break;
                    case nameof(AddressStatsJob):
                        offsetSecs = 240;
                        break;
                    case nameof(MiningStatsJob):
                        offsetSecs = 60;
                        break;
                }

                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"{jobInstance.Name}Trigger")
                    .StartAt(DateTimeOffset.Now.AddSeconds(offsetSecs))
                    .WithSimpleSchedule(jobInstance.Schedule)
                    .Build();

                await scheduler.ScheduleJob(job, trigger);

                offsetSecs += 30;
            }
        }
    }
}