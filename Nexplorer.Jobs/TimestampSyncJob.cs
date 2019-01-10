using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Jobs.Service;

namespace Nexplorer.Jobs
{
    public class TimestampSyncJob : HostedService
    {
        private readonly NexusQuery _nexusQuery;
        private readonly RedisCommand _redisCommand;

        public TimestampSyncJob(NexusQuery nexusQuery, RedisCommand redisCommand, ILogger<TimestampSyncJob> logger) 
            : base(10, logger)
        {
            _nexusQuery = nexusQuery;
            _redisCommand = redisCommand;
        }

        protected override async Task ExecuteAsync()
        {
            var latestStats = await _nexusQuery.GetInfoAsync();

            await _redisCommand.SetAsync(Settings.Redis.TimestampUtcLatest, latestStats.TimeStampUtc);
            await _redisCommand.PublishAsync(Settings.Redis.TimestampUtcLatest, latestStats.TimeStampUtc);

            Logger.LogInformation($"Latest UTC - {latestStats.TimeStampUtc:G}");
        }
    }
}