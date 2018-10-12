using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexplorer.Infrastructure.Bittrex.Models;
using Nexplorer.Infrastructure.Core;

namespace Nexplorer.Infrastructure.Currency
{
    public class CurrencyClient
    {
        private readonly HttpClient _client;

        public CurrencyClient()
        {
            _client = new HttpClient { BaseAddress = new Uri("http://free.currencyconverterapi.com/api/v5/") };

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<decimal> GetConversion(string fromCode, string toCode)
        {
            var url = $"convert?q={fromCode}_{toCode}&compact=y";

            IDictionary<string, JToken> response = await GetAsync(url);

            var val = response?.First().Value.First().First().Value<float>();

            //{"EUR_USD":{"val":1.19403}}
            return response?.First().Value.First().First().Value<decimal>() ?? 0;
        }

        private async Task<JObject> GetAsync(string url)
        {
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return JObject.Parse(json); 
        }
    }
}