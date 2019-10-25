using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Connect.Nexus
{
    public interface INexusHubService
    {
        HubConnectionState State { get; }

        Task<Block> GetBlockAsync(int height, CancellationToken token = default);
        Task<IEnumerable<Block>> GetBlocksAsync(int height, int count, CancellationToken token = default);
        Task<int> GetHeightAsync(CancellationToken token = default);
        Task ConnectAsync(CancellationToken token = default);
        Task RegisterAsync<T>(string name, Func<T, Task> handleAsync);
        Task DisposeAsync();
    }
}