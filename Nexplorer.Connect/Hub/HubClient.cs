using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Nexplorer.Connect.Hub.Core;

namespace Nexplorer.Connect.Hub
{
    public class HubClient : IHubClient
    {
        public HubConnectionState ConnectionState => _connection.State;

        private readonly string _hubName;
        private readonly ILogger _logger;
        private readonly HubConnection _connection;

        public HubClient(HubSettings settings, ILogger logger)
        {
            _logger = logger;

            _connection = new HubConnectionBuilder()
                .WithUrl(settings.Endpoint)
                .WithAutomaticReconnect()
                .Build();

            _connection.Closed += ConnectionOnClosedAsync;
            _connection.Reconnecting += ConnectionOnReconnecting;
            _connection.Reconnected += ConnectionOnReconnected;

            _hubName = settings.Name;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _connection.StartAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to connect to {_hubName} hub");
                _logger.LogError(e.Message);
                if (e.InnerException != null)
                    _logger.LogError(e.InnerException.Message);
            }
        }

        public Task<T> InvokeAsync<T>(IHubInvoke hubInvoke, CancellationToken cancellationToken)
        {
            return _connection.InvokeCoreAsync<T>(hubInvoke.Name, hubInvoke.Args, cancellationToken);
        }

        public void Handle<T>(IHubEvent<T> hubEvent)
        {
            _connection.On<T>(hubEvent.Name, hubEvent.Handle);
        }

        public Task DisposeAsync()
        {
            return _connection.DisposeAsync();
        }

        protected virtual Task ConnectionOnClosedAsync(Exception arg)
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                _logger.LogWarning($"{_hubName} hub has been disconnected");
            }

            return Task.CompletedTask;
        }

        protected virtual Task ConnectionOnReconnecting(Exception error)
        {
            if (_connection.State == HubConnectionState.Reconnecting)
            {
                _logger.LogWarning($"Reconnecting to ${_hubName} hub..");
            }

            return Task.CompletedTask;
        }

        protected virtual Task ConnectionOnReconnected(string connectionId)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                _logger.LogWarning($"{_hubName} hub has successfully reconnected");
            }

            return Task.CompletedTask;
        }
    }
}