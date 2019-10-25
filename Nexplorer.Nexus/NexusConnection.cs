using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Nexus.System.Models;

namespace Nexplorer.Nexus
{
    public class NexusConnection : INexusConnection
    {
        public bool Available { get; private set; }
        public NodeInfo Info { get; set; }
        public Peer[] Peers { get; set; }

        private readonly INexusClient _client;
        private readonly ILogger<NexusConnection> _logger;

        public NexusConnection(INexusClient client, ILogger<NexusConnection> logger)
        {
            _client = client;
            _logger = logger;

            Available = false;
        }

        public async Task<NexusResponse<T>> GetAsync<T>(string path, NexusRequest request, CancellationToken token = default)
        {
            var response = await _client.GetAsync<T>(path, request, token);

            if (!response.CanConnect)
                SetUnavailable();

            return response;
        }

        public async Task<NexusResponse<T>> PostAsync<T>(string path, NexusRequest request, CancellationToken token = default)
        {
            var response = await _client.PostAsync<T>(path, request, token);

            if (!response.CanConnect)
                SetUnavailable();

            return response;
        }

        public async Task RefreshAsync()
        {
            var info = await GetInfoAsync();

            if (!info.CanConnect)
            {
                SetUnavailable();
                return;
            }

            var peers = await GetPeersAsync();

            if (!peers.HasError) 
                Peers = peers.Result.ToArray();

            Info = info.Result;
            Available = true;
        }

        private void SetUnavailable()
        {
            Available = false;
            _logger.LogError($"Nexus endpoint '{_client.NexusEndpoint.Name}' cannot be reached");
        }

        private async Task<NexusResponse<NodeInfo>> GetInfoAsync()
        {
            var info = await _client.GetAsync<NodeInfo>("system/get/info", null);

            if (info.HasError)
                _logger.LogError("Get node info failed");

            return info;
        }

        private async Task<NexusResponse<IEnumerable<Peer>>> GetPeersAsync()
        {
            var peers = await _client.GetAsync<IEnumerable<Peer>>("system/list/peers", null);

            if (peers.HasError)
                _logger.LogError("Get node peers failed");

            return peers;
        }

        private async Task<NexusResponse<IEnumerable<LispEid>>> GetLispEidsAsync()
        {
            var eids = await _client.GetAsync<IEnumerable<LispEid>>("system/list/lisp-eids", null);

            if (eids.HasError)
                _logger.LogError("Get lisp eids info failed");

            return eids;
        }
    }
}