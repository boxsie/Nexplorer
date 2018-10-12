using System.Collections.Generic;
using System.Threading.Tasks;
using Nexplorer.Client.Response;

namespace Nexplorer.Client.Core
{
    public interface INexusClient
    {
        Task<InfoResponse> GetInfoAsync();
        Task<BlockResponse> GetBlockAsync(string hash);
        Task<BlockResponse> GetLastBlockAsync();
        Task<int> GetBlockCountAsync();
        Task<string> GetBlockHashAsync(int height);
        Task<TransactionResponse> GetTransactionAsync(string hash);
        Task<DifficultyResponse> GetNetworkDifficultyAsync();
        Task<List<PeerInfoResponse>> GetPeerInfoAsync();
        Task<MiningInfoResponse> GetMiningInfoAsync();
        Task<SupplyRatesResponse> GetSupplyRatesAsync();
        Task<TrustKeyResponse> GetTrustKeysAsync();
    }
}
