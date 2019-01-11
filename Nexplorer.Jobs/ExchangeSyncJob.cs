using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Context;
using Nexplorer.Data.Map;
using Nexplorer.Domain.Dtos;
using Nexplorer.Infrastructure.Bittrex;
using Nexplorer.Jobs.Service;

namespace Nexplorer.Jobs
{
    public class ExchangeSyncJob : HostedService
    {
        private readonly BittrexClient _bittrexClient;
        private readonly NexusDb _nexusDb;
        private readonly RedisCommand _redis;

        private const string Market = "BTC-NXS";
        private const string UsdMarket = "USDT-BTC";

        private static int _runCount;
        private const int DbSaveRunInterval = 5;

        public ExchangeSyncJob(BittrexClient bittrexClient, NexusDb nexusDb, ILogger<ExchangeSyncJob> logger, RedisCommand redis) 
            : base(5, logger)
        {
            _bittrexClient = bittrexClient;
            _nexusDb = nexusDb;
            _redis = redis;
        }

        protected override async Task ExecuteAsync()
        {
            var nxsSummary = await _bittrexClient.GetMarketSummaryAsync(Market);
            var btcTicket = await _bittrexClient.GetTickerAsync(UsdMarket);

            if (btcTicket != null)
                await _redis.SetAsync(Settings.Redis.BittrexLastUsdBtcPrice, btcTicket.Last);

            if (nxsSummary == null)
            {
                Logger.LogInformation("Bittrex data not availible");
                return;
            }

            await _redis.SetAsync(Settings.Redis.BittrexLastBtcNxsPrice, nxsSummary.Last);

            var summary = nxsSummary.ToBittrexSummary();

            Interlocked.Increment(ref _runCount);
            
            if (_runCount == DbSaveRunInterval)
            {
                await _nexusDb.BittrexSummaries.AddAsync(summary);

                await _nexusDb.SaveChangesAsync();
                
                Interlocked.Exchange(ref _runCount, 0);

                Logger.LogInformation("Latest Bittrex data saved to DB");
            }

            var summaryDto = new BittrexSummaryDto(summary);

            await _redis.PublishAsync(Settings.Redis.BittrexSummaryPubSub, summaryDto);
            await _redis.SetAsync(Settings.Redis.BittrexSummaryPubSub, summaryDto);
        }
    }
}