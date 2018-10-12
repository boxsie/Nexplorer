using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nexplorer.Infrastructure.Core
{
    public abstract class ExchangeClient
    {
        private readonly HttpClient _client;

        protected ExchangeClient(string baseAddress)
        {
            _client = new HttpClient {BaseAddress = new Uri(baseAddress) };

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected async Task<T[]> GetCollectionAsync<T>(string url)
        {
            var response = await _client.GetAsync(url);

            if (response == null || !response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<ExchangeCollectionResponse<T>>(json);

            return obj.Success ? obj.Result : null;
        }

        protected async Task<T> GetAsync<T>(string url)
        {
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return default(T);

            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<ExchangeResponse<T>>(json);

            return obj.Success ? obj.Result : default(T);
        }
    }
}