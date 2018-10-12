using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Data.Query;
using Nexplorer.Sync.Core;
using Nexplorer.Sync.Nexus;
using Quartz;
using Quartz.Impl;

namespace Nexplorer.Sync
{
    public class App
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly BlockSyncCatchup _catchup;
        private readonly ILogger<App> _logger;

        public App(IServiceProvider serviceProvider, BlockSyncCatchup catchup, ILogger<App> logger)
        {
            _serviceProvider = serviceProvider;
            _catchup = catchup;
            _logger = logger;
        }

        public async Task Run()
        {
            await _catchup.Catchup();

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