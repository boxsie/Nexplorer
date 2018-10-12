using System.Linq;
using System.Threading.Tasks;
using Nexplorer.Infrastructure.Bittrex.Models;
using Nexplorer.Infrastructure.Core;

namespace Nexplorer.Infrastructure.Bittrex
{
    public class BittrexClient : ExchangeClient
    {
        public BittrexClient() : base("https://bittrex.com/api/v1.1/public/") { }

        public async Task<BittrexSummaryResponse> GetMarketSummaryAsync(string marketArg)
        {
            var url = $"getmarketsummary?market={marketArg}";

            var response = await GetCollectionAsync<BittrexSummaryResponse>(url);

            return response?.FirstOrDefault();
        }

        public async Task<BittrexTickerResponse> GetTickerAsync(string marketArg)
        {
            var url = $"getticker?market={marketArg}";

            return await GetAsync<BittrexTickerResponse>(url);
        }

        public async Task<BittrexOrderBookResponse> GetOrderBookAsync(string marketArg)
        {
            var url = $"getorderbook?market={marketArg}&type=both";

            return await GetAsync<BittrexOrderBookResponse>(url);
        }

        public async Task<BittrexTradeResponse[]> GetMarketHistoryAsync(string marketArg)
        {
            var url = $"getmarkethistory?market={marketArg}";

            return await GetCollectionAsync<BittrexTradeResponse>(url);
        }
    }
}