using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexplorer.Core;
using Nexplorer.Data;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Connect
{
    public class NexplorerHubService : ScheduledService
    {
        private readonly IHubClient _hub;
        private readonly IBlockDb _blockDb;
        private readonly ILogger<NexplorerHubService> _logger;

        private CancellationToken _token;
        private int _lastNodeHeight;

        public NexplorerHubService(IHubFactory hubFactory, IBlockDb blockDb, ILogger<NexplorerHubService> logger) 
            : base(TimeSpan.FromSeconds(10))
        {
            _hub = hubFactory.Get("Nexplorer");
            _blockDb = blockDb;
            _logger = logger;
            _lastNodeHeight = 0;
        }

        public override async Task Execute()
        {
            await ConnectAsync();

            if (_hub.ConnectionState == HubConnectionState.Connected)
                await Catchup();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _token = cancellationToken;

            _hub.Handle(new HubEvent<int>("PublishNewHeight", OnNewHeightAsync));

            await base.StartAsync(_token);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return _hub.DisposeAsync();
        }

        private async Task ConnectAsync()
        {
            var cnt = 0;
            
            while (_hub.ConnectionState == HubConnectionState.Disconnected)
            {
                cnt++;

                _logger.LogInformation($"Attempt {cnt} to connect to the Nexplorer hub...");
                await _hub.ConnectAsync(_token);

                while (_hub.ConnectionState == HubConnectionState.Connecting)
                    await Task.Delay(TimeSpan.FromSeconds(5), _token);

                if (_hub.ConnectionState == HubConnectionState.Connected)
                    _logger.LogInformation("Successfully connected to the Nexplorer hub");
                else
                {
                    _logger.LogError("Unable to connect to the Nexplorer hub - retrying in 10 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(10), _token);
                }
            }
        }

        private async Task Catchup()
        {
            var lastDbHeight = (await _blockDb.GetHighestAsync())?.Height ?? 0;

            _logger.LogInformation($"Nexus node height:{_lastNodeHeight} | Nexplorer DB height:{lastDbHeight}");

            if (lastDbHeight >= _lastNodeHeight)
                return;

            while (lastDbHeight < _lastNodeHeight)
            {
                var request = new GetBlocksInvoke(lastDbHeight + 1, 100);
                var blocks = (await _hub.InvokeAsync<IEnumerable<Block>>(request, _token)).ToList();

                if (!blocks.Any())
                    return;

                _logger.LogInformation($"Inserting {blocks.Count} blocks");
                await _blockDb.CreateManyAsync(blocks);
                _logger.LogInformation($"Finished inserting blocks {blocks.First().Height} - {blocks.Last().Height} blocks");

                lastDbHeight = (await _blockDb.GetHighestAsync())?.Height ?? 0;

                _logger.LogInformation($"Nexus node height:{_lastNodeHeight} | Nexplorer DB height:{lastDbHeight}");
            }
        }

        private Task OnNewHeightAsync(int newHeight)
        {
            _logger.LogInformation($"New block {newHeight:N0} reported");

            _lastNodeHeight = newHeight;

            return Task.CompletedTask;
        }
    }
}