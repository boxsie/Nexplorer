using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nexplorer.Client.Core;
using Nexplorer.Client.Request;
using Nexplorer.Client.Requests;
using Nexplorer.Client.Response;

namespace Nexplorer.Client
{
    public class NexusClient : INexusClient
    {
        private readonly RpcClient _client;
        private readonly JsonSerializerSettings _settings;
        private readonly ILogger<RpcClient> _logger;
        private readonly bool _outputResponse;

        public NexusClient(ILogger<RpcClient> logger, string connectionString, bool outputResponse = false)
        {
            var connSplit = connectionString.Split(';');

            var url = $"http://{connSplit[0]}";
            var username = connSplit[1].Split('=')[1];
            var password = connSplit[2].Split('=')[1];

            var auth64 = ToBase64($"{username}:{password}");

            _client = new RpcClient(url, auth64, logger);

            _settings = new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc };

            _logger = logger;
            _outputResponse = outputResponse;
        }

        public async Task<InfoResponse> GetInfoAsync()
        {
            return await GetAsync<InfoResponse, InfoRequest>(new InfoRequest());
        }

        public async Task<BlockResponse> GetBlockAsync(string hash)
        {
            return await GetAsync<BlockResponse, BlockRequest>(new BlockRequest(hash));
        }

        public async Task<BlockResponse> GetLastBlockAsync()
        {
            var blockCount = await GetBlockCountAsync();
            var blockHash = await GetBlockHashAsync(blockCount);

            return await GetBlockAsync(blockHash);
        }

        public async Task<int> GetBlockCountAsync()
        {
            return await GetAsync<int, BlockCountRequest>(new BlockCountRequest());
        }

        public async Task<string> GetBlockHashAsync(int height)
        {
            return await GetAsync<string, BlockHashRequest>(new BlockHashRequest(height));
        }

        public async Task<TransactionResponse> GetTransactionAsync(string txHash)
        {
            return await GetAsync<TransactionResponse, TxRequest>(new TxRequest(txHash));
        }

        public async Task<IEnumerable<string>> GetRawMemPool()
        {
            return await GetAsync<IEnumerable<string>, MemPoolRequest>(new MemPoolRequest());
        }

        public async Task<DifficultyResponse> GetNetworkDifficultyAsync()
        {
            return await GetAsync<DifficultyResponse, DifficultyRequest>(new DifficultyRequest());
        }

        public async Task<List<PeerInfoResponse>> GetPeerInfoAsync()
        {
            return await GetAsync<List<PeerInfoResponse>, PeerInfoRequest>(new PeerInfoRequest());
        }

        public async Task<MiningInfoResponse> GetMiningInfoAsync()
        {
            return await GetAsync<MiningInfoResponse, MiningInfoRequest>(new MiningInfoRequest());
        }

        public async Task<TrustKeyResponse> GetTrustKeysAsync()
        {
            return await GetAsync<TrustKeyResponse, TrustKeysRequest>(new TrustKeysRequest());
        }

        public async Task<SupplyRatesResponse> GetSupplyRatesAsync()
        {
            return await GetAsync<SupplyRatesResponse, SupplyRatesRequest>(new SupplyRatesRequest());
        }

        private async Task<T> GetAsync<T, TY>(TY request) where TY : BaseRequest
        {
            try
            {
                var responseJson = await _client.SendAsync(request);

                if (_outputResponse)
                    _logger.LogInformation(responseJson);

                var response = JsonConvert.DeserializeObject<RpcResponse<T>>(responseJson, _settings);

                return response.Result;
            }
            catch (Exception e)
            {
                return default(T);
            }
        }

        private static string ToBase64(string s)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        }
    }
}
