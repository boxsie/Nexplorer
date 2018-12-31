using System.Collections.Generic;
using System.Threading.Tasks;
using Nexplorer.NexusClient.Response;

namespace Nexplorer.NexusClient.Core
{
    public interface INxsClient
    {
        Task<InfoResponse> GetInfoAsync();
        Task<BlockResponse> GetBlockAsync(string hash);
        Task<BlockResponse> GetLastBlockAsync();
        Task<int> GetBlockCountAsync();
        Task<BlockResponse> GetNextBlockAsync(string hash);
        Task<BlockResponse> GetPreviousBlockAsync(string hash);
        Task<string> GetBlockHashAsync(int height);
        Task<TransactionResponse> GetTransactionAsync(string hash);
        Task<DifficultyResponse> GetNetworkDifficultyAsync();
        Task<List<PeerInfoResponse>> GetPeerInfoAsync();
        Task<MiningInfoResponse> GetMiningInfoAsync();
        Task<SupplyRatesResponse> GetSupplyRatesAsync();
        Task<TrustKeyResponse> GetTrustKeysAsync();
        Task<bool> IsBlockHashOnChainAsync(string blockHash);
    }
}
