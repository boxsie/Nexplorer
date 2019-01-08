using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Services;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Web.Hubs
{
    public class AddressMessenger
    {
        private readonly RedisCommand _redisCommand;
        private readonly IHubContext<AddressHub> _addressContext;
        private readonly BlockCacheService _blockCache;

        public AddressMessenger(RedisCommand redisCommand, IHubContext<AddressHub> addressContext, BlockCacheService blockCache)
        {
            _redisCommand = redisCommand;
            _addressContext = addressContext;
            _blockCache = blockCache;

            _redisCommand.Subscribe<AddressStatDto>(Settings.Redis.AddressStatPubSub, OnStatPublishAsync);
        }

        private Task OnStatPublishAsync(AddressStatDto addressStatDto)
        {
            return _addressContext.Clients.All.SendAsync("StatPublish", Helpers.JsonSerialise(addressStatDto));
        }
    }
}