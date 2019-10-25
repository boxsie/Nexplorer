using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Nexplorer.Connect.Hub;
using Nexplorer.Connect.Hub.Core;
using Nexplorer.Connect.Nexus.Invoke;
using Nexplorer.Nexus;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Connect.Nexus
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

        public async Task<Block> GetBlockAsync(int height, CancellationToken token = default)
        {
            var msg = await _hub.InvokeAsync<NexusResponse<Block>>(new GetBlockInvoke(height), token);

            if (HasError(msg))
                _logger.LogError($"Get block {height} failed");

            return msg.Result;
        }

        public async Task<IEnumerable<Block>> GetBlocksAsync(int height, int count, CancellationToken token = default)
        {
            var msg = await _hub.InvokeAsync<NexusResponse<IEnumerable<Block>>>(new GetBlocksInvoke(height, count), token);

            if (HasError(msg))
                _logger.LogError($"Get blocks {height} - {height + count} failed");

            return msg.Result;
        }

        public async Task<int> GetHeightAsync(CancellationToken token = default)
        {
            var msg = await _hub.InvokeAsync<NexusResponse<int>>(new GetHeightInvoke(), token);

            if (HasError(msg))
                _logger.LogError("Get height failed");

            return msg.Result;
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

        private bool HasError(INexusResponse msg)
        {
            if (msg.CanConnect && !msg.IsNodeError && msg.Error == null) 
                return false;

            if (msg.IsNodeError)
            {
                _logger.LogError($"Nexus error code: {msg.Error.Code}");
                _logger.LogError(msg.Error.Message);
            }
            else if (!msg.CanConnect)
                _logger.LogError("Node is unreachable");

            return true;
        }
    }
}