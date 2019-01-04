using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Nexplorer.Jobs.Service
{
    public static class JobService
    {
        private static Dictionary<Type, HostedService> _jobs;
       
        public static void Init(IServiceCollection services)
        {
            var jobTypes = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(HostedService)) && !x.IsAbstract);

            foreach (var jobType in jobTypes)
                services.AddSingleton(typeof(IHostedService), jobType);
        }

        public static void Start(IEnumerable<IHostedService> jobs)
        {
            _jobs = jobs.ToDictionary(x => x.GetType(), y => (HostedService)y);
        }

        public static async Task StartJob(Type jobType, int delaySeconds = 0)
        {
            if (!_jobs.ContainsKey(jobType))
                return;

            var job = _jobs[jobType];

            if (job.IsRunning)
                return;

            if (delaySeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

            await job.StartAsync(new CancellationToken());
        }

        public static async Task StopJob(Type jobType)
        {
            if (!_jobs.ContainsKey(jobType))
                return;

            var job = _jobs[jobType];

            await job.StopAsync(new CancellationToken(true));
        }
    }
}