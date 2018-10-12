using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nexplorer.Config;
using Nexplorer.Core;

namespace Nexplorer.Web.Hubs
{
    public class LayoutHub : Hub
    {
        private readonly RedisCommand _redisCommand;

        public LayoutHub(RedisCommand redisCommand)
        {
            _redisCommand = redisCommand;
        }

        public async Task<DateTime> GetLatestTimestampUtc()
        {
            return await _redisCommand.GetAsync<DateTime>(Settings.Redis.TimestampUtcLatest);
        }
    }
}