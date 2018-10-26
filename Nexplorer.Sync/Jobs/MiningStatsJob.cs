using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Infrastructure.Geolocate;
using Nexplorer.Sync.Core;
using Quartz;

namespace Nexplorer.Sync.Jobs
{
    public class MiningStatsJob : SyncJob
    {
        private readonly RedisCommand _redisCommand;
        private readonly NexusQuery _nexusQuery;
        private readonly StatQuery _statQuery;

        public MiningStatsJob(ILogger<MiningStatsJob> logger, RedisCommand redisCommand, NexusQuery nexusQuery, StatQuery statQuery)
            : base(logger, 10)
        {
            _redisCommand = redisCommand;
            _nexusQuery = nexusQuery;
            _statQuery = statQuery;
        }

        protected override async Task<string> ExecuteAsync()
        {
            var latestStats = await _nexusQuery.GetInfoAsync();

            await _redisCommand.SetAsync(Settings.Redis.TimestampUtcLatest, latestStats.TimeStampUtc);
            await _redisCommand.PublishAsync(Settings.Redis.TimestampUtcLatest, latestStats.TimeStampUtc);
            
            await UpdateMiningStatsAsync();
            await UpdateMiningInfoAsync();

            return "Updated mining stats";
        }

        private async Task UpdateMiningStatsAsync()
        {
            var channelStats = await _statQuery.GetChannelStatsAsync();

            var statDto = new MiningStatDto
            {
                ChannelStats = channelStats?.ToDictionary(x => x.Channel.ToLower()),
                SupplyRate = await _nexusQuery.GetSupplyRate()
            };

            await _redisCommand.SetAsync(Settings.Redis.SupplyRatesLatest, statDto.SupplyRate);
            await _redisCommand.SetAsync(Settings.Redis.ChannelStatsLatest, statDto.ChannelStats);

            await _redisCommand.PublishAsync(Settings.Redis.MiningStatPubSub, statDto);
        }

        private async Task UpdateMiningInfoAsync()
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
        }
    }
}