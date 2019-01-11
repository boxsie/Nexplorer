using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Jobs.Service;

namespace Nexplorer.Jobs
{
    public class MiningStatsJob : HostedService
    {
        private readonly NexusQuery _nexusQuery;
        private readonly RedisCommand _redisCommand;
        private readonly StatQuery _statQuery;

        public MiningStatsJob(NexusQuery nexusQuery, RedisCommand redisCommand, StatQuery statQuery, ILogger<MiningStatsJob> logger)
            : base(10, logger)
        {
            _nexusQuery = nexusQuery;
            _redisCommand = redisCommand;
            _statQuery = statQuery;
        }

        protected override async Task ExecuteAsync()
        {
            var sw = new Stopwatch();
            sw.Start();
            
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

            sw.Stop();
            Logger.LogInformation($"Mining stats job completed in {sw.Elapsed:c}");
        }
    }
}