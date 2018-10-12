using System.Threading.Tasks;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Domain.Enums;
using Nexplorer.Infrastructure.Currency;

namespace Nexplorer.Data.Query
{
    public class CurrencyQuery
    {
        private readonly CurrencyClient _currencyClient;
        private readonly RedisCommand _redis;

        public CurrencyQuery(CurrencyClient currencyClient, RedisCommand redis)
        {
            _currencyClient = currencyClient;
            _redis = redis;
        }

        public async Task<decimal> ConvertFromUSDAsync(Currency currency)
        {
            if (currency == Currency.USD)
                return 1m;

            return await _currencyClient.GetConversion(Currency.USD.ToString(), currency.ToString());
        }

        public async Task<decimal> ConvertToUSDAsync(Currency currency)
        {
            if (currency == Currency.USD)
                return 1m;

            return await _currencyClient.GetConversion(currency.ToString(), Currency.USD.ToString());
        }

        public async Task<decimal> ConvertToBTCAsync(Currency currency)
        {
            if (currency == Currency.USD)
                return 1m;

            return await _currencyClient.GetConversion(Currency.USD.ToString(), currency.ToString());
        }

        public async Task<decimal> ConvertFromBTCAsync(Currency currency)
        {
            if (currency == Currency.USD)
                return 1m;

            return await _currencyClient.GetConversion(currency.ToString(), Currency.USD.ToString());
        }

        public async Task<double> GetLatestBTCPriceInUSDAsync()
        {
            return await _redis.GetAsync<double>(Settings.Redis.BittrexLastUsdBtcPrice);
        }

        public async Task<double> GetLatestNXSPriceInBTCAsync()
        {
            return await _redis.GetAsync<double>(Settings.Redis.BittrexLastBtcNxsPrice);
        }
    }
}