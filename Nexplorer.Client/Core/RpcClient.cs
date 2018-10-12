using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Nexplorer.Client.Core
{
    public class RpcClient
    {
        private readonly Uri _baseUri;
        private readonly string _auth64;
        private readonly ILogger<RpcClient> _logger;

        public RpcClient(string uri, string auth64, ILogger<RpcClient> logger)
        {
            _baseUri = new Uri(uri);
            _auth64 = auth64;
            _logger = logger;
        }

        public async Task<string> SendAsync<T>(T rpcRequest) where T : BaseRequest
        {
            using (var httpClient = GetHttpClientAsync())
            {
                var httpResponseMessage = await httpClient.PostAsync(_baseUri, rpcRequest.GetContent());
                
                return await httpResponseMessage.Content.ReadAsStringAsync();
            }
        }

        private HttpClient GetHttpClientAsync()
        {
            var httpClient = new HttpClient { BaseAddress = _baseUri };

            httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Basic {_auth64}");
            httpClient.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");

            return httpClient;
        }
    }
}
