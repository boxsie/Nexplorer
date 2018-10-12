using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nexplorer.Config;
using Nexplorer.Core;

namespace Nexplorer.Web.Hubs
{
    public class LayoutMessenger
    {
        private readonly RedisCommand _redisCommand;
        private readonly IHubContext<LayoutHub> _layoutContext;

        public LayoutMessenger(RedisCommand redisCommand, IHubContext<LayoutHub> layoutContext)
        {
            _redisCommand = redisCommand;
            _layoutContext = layoutContext;

            _redisCommand.Subscribe<DateTime>(Settings.Redis.TimestampUtcLatest, OnLatestTimestampUtc);
        }

        private Task OnLatestTimestampUtc(DateTime timestamp)
        {
            return _layoutContext.Clients.All.SendAsync("UpdateTimestampUtc", timestamp);
        }
    }
}