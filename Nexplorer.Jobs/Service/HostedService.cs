using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Nexplorer.Jobs.Service
{
    public abstract class HostedService : IHostedService
    {
        public bool IsRunning => _executingTask != null && _executingTask.IsCompleted;

        private readonly TimeSpan _jobInterval;

        private Task _executingTask;
        private CancellationTokenSource _cts;

        protected abstract Task ExecuteAsync();
        
        protected HostedService(int intervalSeconds)
        {
            _jobInterval = TimeSpan.FromSeconds(intervalSeconds);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _executingTask = ExecuteIntervalAsync(_cts.Token);

            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
                return;

            _cts.Cancel();

            await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));

            cancellationToken.ThrowIfCancellationRequested();
        }

        private async Task ExecuteIntervalAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                if (_jobInterval == TimeSpan.Zero)
                    await StopAsync(cancellationToken);
                else
                    await Task.Delay(_jobInterval, cancellationToken);
            }
        }
    }
}