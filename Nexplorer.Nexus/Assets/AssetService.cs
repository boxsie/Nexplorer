using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Nexus.Assets.Models;

namespace Nexplorer.Nexus.Assets
{
    public class AssetService : IAssetService
    {
        private readonly INexusConnection _nxs;
        private readonly ILogger<AssetService> _logger;

        public AssetService(INexusConnection nxs, ILogger<AssetService> logger)
        {
            _nxs = nxs;
            _logger = logger;
        }

        public async Task<NexusResponse<AssetInfo>> GetAssetAsync(Asset asset, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(asset?.Address))
                throw new ArgumentException("Address is required");

            var (key, val) = asset.GetKeyVal();

            var request = new NexusRequest(new Dictionary<string, string> {{key, val}});

            var msg = await _nxs.PostAsync<AssetInfo>("assets/get", request, token);

            if (msg.HasError)
                _logger.LogError($"{key} retrieval failed");

            return msg;
        }

        public async Task<NexusResponse<IEnumerable<AssetInfo>>> GetAssetHistoryAsync(Asset asset,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(asset?.Address))
                throw new ArgumentException("Address is required");
            
            var (key, val) = asset.GetKeyVal();
            
            var request = new NexusRequest(new Dictionary<string, string> { { key, val } });

            var msg = await _nxs.PostAsync<IEnumerable<AssetInfo>>("assets/history", request, token);

            if (msg.HasError)
                _logger.LogError($"Get asset {val} history failed");

            return msg;
        }
    }
}