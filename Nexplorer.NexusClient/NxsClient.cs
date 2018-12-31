using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexplorer.NexusClient.Core;
using Nexplorer.NexusClient.Request;
using Nexplorer.NexusClient.Response;

namespace Nexplorer.NexusClient
{
    public class NxsClient : INxsClient
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerSettings _settings;
        private readonly bool _outputErrors;

        public NxsClient(string connectionString, bool outputErrors = false)
        {
            var connSplit = connectionString.Split(';');

            var url = $"http://{connSplit[0]}";
            var username = connSplit[1].Split('=')[1];
            var password = connSplit[2].Split('=')[1];
            var auth64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

            _client = CreateClient(new Uri(url), auth64);

            _settings = new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc };

            _outputErrors = outputErrors;
        }

        public async Task<InfoResponse> GetInfoAsync()
        {
            return await GetAsync<InfoResponse, InfoRequest>(new InfoRequest());
        }

        public async Task<BlockResponse> GetBlockAsync(string hash)
        {
            return await GetAsync<BlockResponse, BlockRequest>(new BlockRequest(hash));
        }

        public async Task<string> GetBlockHashAsync(int height)
        {
            return await GetAsync<string, BlockHashRequest>(new BlockHashRequest(height));
        }

        public async Task<BlockResponse> GetLastBlockAsync()
        {
            var blockCount = await GetBlockCountAsync();
            var blockHash = await GetBlockHashAsync(blockCount);

            return await GetBlockAsync(blockHash);
        }

        public async Task<BlockResponse> GetNextBlockAsync(string hash)
        {
            var block = await GetBlockAsync(hash);

            return await GetBlockAsync(block.NextBlockHash);
        }

        public async Task<BlockResponse> GetPreviousBlockAsync(string hash)
        {
            var block = await GetBlockAsync(hash);

            return await GetBlockAsync(block.PreviousBlockHash);
        }

        public async Task<int> GetBlockCountAsync()
        {
            return await GetAsync<int, BlockCountRequest>(new BlockCountRequest());
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

        public async Task<bool> IsBlockHashOnChainAsync(string blockHash)
        {
            var response = await GetAsync<bool?, IsOrphanRequest>(new IsOrphanRequest(blockHash));

            return response.HasValue && !response.Value;
        }

        private async Task<T> GetAsync<T, TY>(TY request) where TY : BaseRequest
        {
            var response = await GetResponseAsync<T, TY>(request);

            if (response.Error == null)
                return response.Result;

            if (_outputErrors)
            {
                Console.WriteLine(response.Error.Code);
                Console.WriteLine(response.Error.Message);
            }

            return default(T);
        }

        private async Task<RpcResponse<T>> GetResponseAsync<T, TY>(TY request) where TY : BaseRequest
        {
            try
            {
                var httpResponseMessage = await _client.PostAsync(_client.BaseAddress, request.GetContent());

                var responseJson = await httpResponseMessage.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<RpcResponse<T>>(responseJson, _settings);
            }
            catch (Exception e)
            {
                if (_outputErrors)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.InnerException);
                    Console.WriteLine(e.StackTrace);
                }

                return null;
            }
        }

        private static HttpClient CreateClient(Uri uri, string auth64)
        {
            var client = new HttpClient { BaseAddress = uri };

            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Basic {auth64}");
            client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");

            return client;
        }
    }
}
