using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Nexus.Accounts.Models;
using Nexplorer.Nexus.Assets.Models;
using Nexplorer.Nexus.Extensions;
using Nexplorer.Nexus.Nexus;
using Nexplorer.Nexus.Tokens.Models;

namespace Nexplorer.Nexus.Assets
{
    public class AssetService : NexusService
    {
        public AssetService(NexusNode node, ILogger<NexusService> log) : base(node, log) { }

        public async Task<Asset> CreateAssetAsync(Asset asset, NexusUser user, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            user.Validate();
            asset.Validate();

            var request = new NexusRequest(new Dictionary<string, string>
            {
                {"pin", user.Pin.ToString()},
                {"session", user.GenesisId.Session},
                {"data", asset.Data},
                {"name", asset.Name}
            });

            var response = await PostAsync<NexusCreationResponse>("assets/create", request, token);

            if (string.IsNullOrWhiteSpace(response?.Address))
                throw new InvalidOperationException($"{asset.Name} creation failed");

            asset.Address = response.Address;
            asset.TxId = response.TxId;
            asset.Genesis = user.GenesisId.Genesis;
            asset.CreatedOn = DateTime.Now;

            return asset;
        }

        public async Task<AssetInfo> GetAssetAsync(Asset asset, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(asset?.Address))
                throw new ArgumentException("Address is required");

            var (key, val) = asset.GetKeyVal();

            var request = new NexusRequest(new Dictionary<string, string> {{key, val}});

            var assetInfo = await PostAsync<AssetInfo>("assets/get", request, token);

            if (assetInfo == null)
                throw new InvalidOperationException($"{key} retrieval failed");

            return assetInfo;
        }

        public async Task<Asset> TransferAssetAsync(Asset asset, NexusUser fromUser, GenesisId toUserGenesis, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(toUserGenesis?.Genesis))
                throw new ArgumentException("Genesis is required");

            return await TransferAssetAsync(asset, fromUser, ("destination", toUserGenesis.Genesis), token);
        }

        public async Task<Asset> TransferAssetAsync(Asset asset, NexusUser fromUser, string toUsername, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(toUsername))
                throw new ArgumentException("Username is required");

            return await TransferAssetAsync(asset, fromUser, ("username", toUsername), token);
        }

        public async Task<object> TokeniseAsset(Asset asset, Token nexusToken, NexusUser user, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            user.Validate();
            asset.Validate();
            nexusToken.Validate();

            var (aKey, aVal) = asset.GetKeyVal("asset_address", "asset_name");
            var (tKey, tVal) = nexusToken.GetKeyVal("token_address", "token_name");

            var request = new NexusRequest(new Dictionary<string, string>
            {
                {"pin", user.Pin.ToString()},
                {"session", user.GenesisId.Session},
                {aKey, aVal},
                {tKey, tVal}
            });

            return await PostAsync<Asset>("assets/tokenize", request, token);
        }

        public async Task<IEnumerable<AssetInfo>> GetAssetHistoryAsync(Asset asset, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(asset?.Address))
                throw new ArgumentException("Address is required");
            
            var (key, val) = asset.GetKeyVal();
            
            var request = new NexusRequest(new Dictionary<string, string> { { key, val } });

            var assetHistory = await PostAsync<IEnumerable<AssetInfo>>("assets/history", request, token);

            if (assetHistory == null)
                throw new InvalidOperationException($"Get asset {val} history failed");

            return assetHistory;
        }

        private async Task<Asset> TransferAssetAsync(Asset asset, NexusUser fromUser, (string, string) toUserKeyVal, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            fromUser.Validate();
            asset.Validate();

            var (key, val) = asset.GetKeyVal();

            var request = new NexusRequest(new Dictionary<string, string>
            {
                {"pin", fromUser.Pin.ToString()},
                {"session", fromUser.GenesisId.Session},
                {toUserKeyVal.Item1, toUserKeyVal.Item2},
                {key, val}
            });

            var newId = await PostAsync<Asset>("assets/transfer", request, token);

            if (newId == null)
                throw new InvalidOperationException($"{asset.Name} transfer from {fromUser.GenesisId.Genesis} to {toUserKeyVal.Item2} failed");

            return asset;
        }
    }
}