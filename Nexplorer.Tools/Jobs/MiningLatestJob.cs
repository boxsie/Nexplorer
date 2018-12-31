using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;

namespace Nexplorer.Tools.Jobs
{
    public class MiningLatestJob
    {
        private readonly ILogger<MiningLatestJob> _logger;
        private readonly RedisCommand _redisCommand;
        private readonly NexusQuery _nexusQuery;

        public MiningLatestJob(ILogger<MiningLatestJob> logger, RedisCommand redisCommand, NexusQuery nexusQuery)
        {
            _logger = logger;
            _redisCommand = redisCommand;
            _nexusQuery = nexusQuery;
        }

        public async Task Excecute()
        {
            var miningInfo = await _nexusQuery.GetMiningInfoAsync();

            await _redisCommand.SetAsync(Settings.Redis.MiningInfoLatest, miningInfo);

        }
    }
}