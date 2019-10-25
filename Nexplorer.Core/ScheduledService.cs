using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Nexplorer.Core
{
    public abstract class ScheduledService : IHostedService
    {
        protected readonly ILogger<ScheduledService> Logger;

        private readonly TimeSpan _interval;
        private Timer _timer;

        protected ScheduledService(TimeSpan interval, ILogger<ScheduledService> logger)
        {
            _interval = interval;

            Logger = logger;
        }

        public abstract Task Execute();

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(Execute, null, TimeSpan.Zero, _interval);
            return Task.CompletedTask;
        }

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async void Execute(object state)
        {
            try
            {
                _timer.Change(Timeout.Infinite, 0);

                await Execute();

                _timer.Change(_interval, TimeSpan.Zero);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                _timer.Change(_interval, TimeSpan.Zero);
            }
        }
    }
}