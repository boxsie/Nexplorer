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

        public async Task<T> GetAsync<T>(string path, NexusRequest request, CancellationToken token = default)
        {
            if (!CheckAvailability(path))
                return default;

            var response = await _client.GetAsync<T>(path, request, token);

            return ParseResponse(response) 
                ? response.Result 
                : default;
        }

        public async Task<T> PostAsync<T>(string path, NexusRequest request, CancellationToken token = default)
        {
            if (!CheckAvailability(path))
                return default;

            var response = await _client.PostAsync<T>(path, request, token);

            return ParseResponse(response)
                ? response.Result
                : default;
        }

        public async Task RefreshAsync()
        {
            var info = await GetInfoAsync();

            if (info == null)
            {
                SetUnavailable();
                return;
            }

            var peers = await GetPeersAsync();

            if (peers != null) 
                Peers = peers.ToArray();

            Info = info;
            Available = true;
        }

        private bool CheckAvailability(string path)
        {
            if (Available) 
                return true;
            
            _logger.LogError($"{path} request failed - Nexus node is not available at this time");
            return false;
        }

        private bool ParseResponse<T>(NexusResponse<T> response)
        {
            if (response.CanConnect) 
                return response.Error == null && !response.IsNodeError;
            
            SetUnavailable();
            return false;
        }

        private void SetUnavailable()
        {
            Available = false;
            _logger.LogError($"Nexus endpoint '{_client.NexusEndpoint.Name}' cannot be reached");
        }

        private async Task<NodeInfo> GetInfoAsync()
        {
            var info = await _client.GetAsync<NodeInfo>("system/get/info", null);

            if (info == null)
                _logger.LogError("Get node info failed");

            return info?.Result;
        }

        private async Task<IEnumerable<Peer>> GetPeersAsync()
        {
            var peers = await _client.GetAsync<IEnumerable<Peer>>("system/list/peers", null);

            if (peers == null)
                _logger.LogError("Get node peers failed");

            return peers?.Result;
        }

        private async Task<IEnumerable<LispEid>> GetLispEidsAsync()
        {
            var eids = await _client.GetAsync<IEnumerable<LispEid>>("system/list/lisp-eids", null);

            if (eids == null)
                _logger.LogError("Get lisp eids info failed");

            return eids?.Result;
        }
    }
}