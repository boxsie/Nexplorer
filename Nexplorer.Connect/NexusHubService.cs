using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Connect
{
    public class NexusHubService : INexusHubService
    {
        public HubConnectionState State => _hub.ConnectionState;

        private readonly ILogger<NexusHubService> _logger;
        private readonly IHubClient _hub;

        public NexusHubService(IHubFactory hubFactory, ILogger<NexusHubService> logger)
        {
            _logger = logger;
            _hub = hubFactory.Get("Nexplorer");
        }

        public Task<Block> GetBlockAsync(int height, CancellationToken token = default)
        {
            return _hub.InvokeAsync<Block>(new GetBlockInvoke(height), token);
        }

        public Task<IEnumerable<Block>> GetBlocksAsync(int height, int count, CancellationToken token = default)
        {
            return _hub.InvokeAsync<IEnumerable<Block>>(new GetBlocksInvoke(height, count), token);
        }

        public Task<int?> GetHeightAsync(CancellationToken token = default)
        {
            return _hub.InvokeAsync<int?>(new GetHeightInvoke(), token);
        }

        public async Task ConnectAsync(CancellationToken token = default)
        {
            var cnt = 0;

            while (_hub.ConnectionState == HubConnectionState.Disconnected)
            {
                cnt++;

                _logger.LogInformation($"Attempt {cnt} to connect to the Nexplorer hub...");
                await _hub.ConnectAsync(token);

                while (_hub.ConnectionState == HubConnectionState.Connecting)
                    await Task.Delay(TimeSpan.FromSeconds(5), token);

                if (_hub.ConnectionState == HubConnectionState.Connected)
                    _logger.LogInformation("Successfully connected to the Nexplorer hub");
                else
                {
                    _logger.LogError("Unable to connect to the Nexplorer hub - retrying in 10 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(10), token);
                }
            }
        }

        public Task RegisterAsync<T>(string name, Func<T, Task> handleAsync)
        {
            _hub.Handle(new HubEvent<T>(name, handleAsync));

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            return _hub.DisposeAsync();
        }
    }
}