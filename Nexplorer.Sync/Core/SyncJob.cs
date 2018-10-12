using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Nexplorer.Sync.Core
{
    [DisallowConcurrentExecution]
    public abstract class SyncJob : IJob
    {
        public string Name => GetType().Name;
        public Action<SimpleScheduleBuilder> Schedule => x => x.WithIntervalInSeconds(_intervalSeconds).RepeatForever();

        protected readonly ILogger Logger;
        private readonly int _intervalSeconds;
        
        protected SyncJob(ILogger logger, int intervalSeconds)
        {
            Logger = logger;
            _intervalSeconds = intervalSeconds == 0 ? 30 : intervalSeconds;
        }

        protected abstract Task<string> ExecuteAsync();

        public ITrigger GetTrigger()
        {
            return TriggerBuilder.Create()
                .WithIdentity(Name)
                .StartNow()
                .WithSimpleSchedule(Schedule)
                .Build();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = await ExecuteAsync();

                if (result != null)
                    Logger.LogInformation($"{stopwatch.Elapsed} - {result}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"{ex.Message}\r\n{ex.InnerException}");
                Logger.LogError(string.Join("\r\n", ex.StackTrace));
            }
        }
    }
}