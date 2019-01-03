using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Sync.Hangfire
{
    public class StatsJob
    {
        public static readonly TimeSpan TimestampJobInterval = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan MiningStatsJobInterval = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan MiningHistoricalJobInterval = TimeSpan.FromSeconds(30);

        private readonly ILogger<StatsJob> _logger;
        private readonly RedisCommand _redisCommand;
        private readonly NexusQuery _nexusQuery;
        private readonly StatQuery _statQuery;

        public StatsJob(ILogger<StatsJob> logger, RedisCommand redisCommand, NexusQuery nexusQuery, StatQuery statQuery)
        {
            _logger = logger;
            _redisCommand = redisCommand;
            _nexusQuery = nexusQuery;
            _statQuery = statQuery;
        }

        public async Task UpdateTimestamp()
        {
            var latestStats = await _nexusQuery.GetInfoAsync();

            await _redisCommand.SetAsync(Settings.Redis.TimestampUtcLatest, latestStats.TimeStampUtc);
            await _redisCommand.PublishAsync(Settings.Redis.TimestampUtcLatest, latestStats.TimeStampUtc);

            BackgroundJob.Schedule<StatsJob>(x => x.UpdateTimestamp(), TimestampJobInterval);
        }

        public async Task PublishMiningStatsAsync()
        {
            var channelStats = await _statQuery.GetChannelStatsAsync();

            var statDto = new MiningStatDto
            {
                ChannelStats = channelStats?.ToDictionary(x => x.Channel.ToLower()),
                SupplyRate = await _nexusQuery.GetSupplyRate()
            };

            var diffs = channelStats?.ToDictionary(x => x.Channel.ToLower(), y => y.Difficulty);

            await _redisCommand.SetAsync(Settings.Redis.SupplyRatesLatest, statDto.SupplyRate);
            await _redisCommand.SetAsync(Settings.Redis.ChannelStatsLatest, statDto.ChannelStats);
            await _redisCommand.SetAsync(Settings.Redis.DifficultyStatPubSub, diffs);

            await _redisCommand.PublishAsync(Settings.Redis.MiningStatPubSub, statDto);
            await _redisCommand.PublishAsync(Settings.Redis.DifficultyStatPubSub, diffs);

            BackgroundJob.Schedule<StatsJob>(x => x.PublishMiningStatsAsync(), MiningStatsJobInterval);
        }

        public async Task UpdateMiningHistoricalAsync()
        {
            var miningInfo = await _nexusQuery.GetMiningInfoAsync();

            var recentMiningInfos = await _redisCommand.GetAsync<List<MiningInfoDto>>(Settings.Redis.MiningInfo10Mins) ??
                                    new List<MiningInfoDto>();

            if (recentMiningInfos.All(x => x.CreatedOn != miningInfo.CreatedOn))
            {
                recentMiningInfos.Add(miningInfo);
                recentMiningInfos.RemoveAll(x => x.CreatedOn < DateTime.UtcNow.AddMinutes(-10));

                await _redisCommand.SetAsync(Settings.Redis.MiningInfo10Mins, recentMiningInfos);
            }

            await _redisCommand.SetAsync(Settings.Redis.MiningInfoLatest, miningInfo);

            BackgroundJob.Schedule<StatsJob>(x => x.UpdateMiningHistoricalAsync(), MiningHistoricalJobInterval);
        }
    }
}