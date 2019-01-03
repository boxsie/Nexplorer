using System.Linq;
using Hangfire;
using Hangfire.Storage;

namespace Nexplorer.Sync.Hangfire.Core
{
    public static class HangfireExtensions
    {
        public static void PurgeJobs(this IMonitoringApi monitor)
        {
            //RecurringJobs
            JobStorage.Current.GetConnection().GetRecurringJobs().ForEach(xx => BackgroundJob.Delete(xx.Id));

            //ProcessingJobs
            monitor.ProcessingJobs(0, int.MaxValue).ForEach(xx => BackgroundJob.Delete(xx.Key));

            //ScheduledJobs
            monitor.ScheduledJobs(0, int.MaxValue).ForEach(xx => BackgroundJob.Delete(xx.Key));

            //EnqueuedJobs
            monitor.Queues().ToList().ForEach(xx => monitor.EnqueuedJobs(xx.Name, 0, int.MaxValue).ForEach(x => BackgroundJob.Delete(x.Key)));
        }
    }
}