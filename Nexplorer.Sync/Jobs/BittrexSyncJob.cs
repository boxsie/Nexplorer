using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Context;
using Nexplorer.Data.Map;
using Nexplorer.Domain.Dtos;
using Nexplorer.Infrastructure.Bittrex;
using Nexplorer.Sync.Core;
using StackExchange.Redis;

namespace Nexplorer.Sync.Jobs
{
    public class BittrexSyncJob : SyncJob
    {
        private readonly BittrexClient _bittrexClient;
        private readonly NexusDb _nexusDb;
        private readonly RedisCommand _redis;

        private const string Market = "BTC-NXS";
        private const string UsdMarket = "USDT-BTC";
        
        private static int _runCount;
        private const int DbSaveRunInterval = 5;

        public BittrexSyncJob(BittrexClient bittrexClient, NexusDb nexusDb, ILogger<BittrexSyncJob> logger, RedisCommand redis)
            : base(logger, 5)
        {
            _bittrexClient = bittrexClient;
            _nexusDb = nexusDb;
            _redis = redis;
        }

        protected override async Task<string> ExecuteAsync()
        {
            var nxsSummary = await _bittrexClient.GetMarketSummaryAsync(Market);
            var btcTicket = await _bittrexClient.GetTickerAsync(UsdMarket);

            if (btcTicket != null)
                await _redis.SetAsync(Settings.Redis.BittrexLastUsdBtcPrice, btcTicket.Last);

            if (nxsSummary == null)
                return "Bittrex data not availible";

            await _redis.SetAsync(Settings.Redis.BittrexLastBtcNxsPrice, nxsSummary.Last);

            var summary = nxsSummary.ToBittrexSummary();

            Interlocked.Increment(ref _runCount);

            var dataSaved = false;

            if (_runCount == DbSaveRunInterval)
            {
                await _nexusDb.BittrexSummaries.AddAsync(summary);

                await _nexusDb.SaveChangesAsync();

                dataSaved = true;

                Interlocked.Exchange(ref _runCount, 0);
            }

            var summaryDto = new BittrexSummaryDto(summary);

            await _redis.PublishAsync(Settings.Redis.BittrexSummaryPubSub, summaryDto);
            await _redis.SetAsync(Settings.Redis.BittrexSummaryPubSub, summaryDto);

            return dataSaved ? "Latest Bittrex data saved to DB" : null;
        }
    }
}