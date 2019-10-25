using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Nexplorer.Connect.Hub.Core
{
    public interface IHubClient
    {
        HubConnectionState ConnectionState { get; }

        Task<T> InvokeAsync<T>(IHubInvoke invoke, CancellationToken cancellationToken);
        void Handle<T>(IHubEvent<T> @event);
        Task ConnectAsync(CancellationToken cancellationToken);
        Task DisposeAsync();
    }
}