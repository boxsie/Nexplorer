using System;
using Quartz;
using Quartz.Simpl;
using Quartz.Spi;

namespace Nexplorer.Sync.Core
{
    public class JobFactory : SimpleJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public JobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return (IJob)_serviceProvider.GetService(bundle.JobDetail.JobType);
        }
    }
}