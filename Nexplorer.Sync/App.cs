using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Data.Cache.Block;
using Nexplorer.Data.Query;
using Nexplorer.Sync.Core;
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
        private readonly BlockRewardCatchup _blockRewardCatchup;
        private readonly BlockCacheBuild _blockCacheBuild;

        public App(ILogger<App> logger, IServiceProvider serviceProvider, BlockSyncCatchup blockCatchup, 
            AddressAggregateCatchup addressAggregateCatchup, BlockRewardCatchup blockRewardCatchup, BlockCacheBuild blockCacheBuild)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _blockCatchup = blockCatchup;
            _addressAggregateCatchup = addressAggregateCatchup;
            _blockRewardCatchup = blockRewardCatchup;
            _blockCacheBuild = blockCacheBuild;
        }

        public async Task Run()
        {
            await _blockCatchup.Catchup();
            await _addressAggregateCatchup.Catchup();
            await _blockRewardCatchup.Catchup();
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

            var jobTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(SyncJob)));

            foreach (var jt in jobTypes)
            {
                var jobInstance = (SyncJob)_serviceProvider.GetService(jt);

                var job = JobBuilder.Create(jt)
                    .WithIdentity($"{jt.Name}Job")
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"{jt.Name}Trigger")
                    .StartNow()
                    .WithSimpleSchedule(jobInstance.Schedule)
                    .Build();

                await scheduler.ScheduleJob(job, trigger);
            }
        }
    }
}