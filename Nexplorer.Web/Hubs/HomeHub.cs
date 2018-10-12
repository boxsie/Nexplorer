using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;

namespace Nexplorer.Web.Hubs
{
    public class HomeHub : Hub
    {
        private readonly RedisCommand _redisCommand;
        private readonly ExchangeQuery _exchangeQuery;

        public HomeHub(RedisCommand redisCommand, ExchangeQuery exchangeQuery)
        {
            _redisCommand = redisCommand;
            _exchangeQuery = exchangeQuery;
        }

        public async Task<string> GetLatestBittrexSummary()
        {
            return Helpers.JsonSerialise(await _exchangeQuery.GetLatestBittrexSummaryAsync());
        }

        public async Task<int> GetBlockCountLastDay()
        {
            return await _redisCommand.GetAsync<int>(Settings.Redis.BlockCount24Hours);
        }

        public async Task<int> GetTransactionCountLastDay()
        {
            return await _redisCommand.GetAsync<int>(Settings.Redis.TransactionCount24Hours);
        }
    }
}