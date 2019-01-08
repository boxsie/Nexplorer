using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Data.Services;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Web.Hubs
{
    public class MiningMessenger
    {
        private readonly RedisCommand _redisCommand;
        private readonly IHubContext<MiningHub> _miningContext;
        private readonly BlockCacheService _blockCache;

        public MiningMessenger(RedisCommand redisCommand, IHubContext<MiningHub> miningContext,
            BlockCacheService blockCache)
        {
            _redisCommand = redisCommand;
            _miningContext = miningContext;
            _blockCache = blockCache;

            _redisCommand.Subscribe<MiningStatDto>(Settings.Redis.MiningStatPubSub, OnStatPublishAsync);
        }

        private Task OnStatPublishAsync(MiningStatDto miningStatDto)
        {
            return _miningContext.Clients.All.SendAsync("StatPublish", Helpers.JsonSerialise(miningStatDto));
        }
    }
}