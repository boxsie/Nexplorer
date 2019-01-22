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

        public AddressMessenger(RedisCommand redisCommand, IHubContext<AddressHub> addressContext)
        {
            _redisCommand = redisCommand;
            _addressContext = addressContext;

            _redisCommand.Subscribe<AddressStatDto>(Settings.Redis.AddressStatPubSub, OnStatPublishAsync);
        }

        private Task OnStatPublishAsync(AddressStatDto addressStatDto)
        {
            return _addressContext.Clients.All.SendAsync("StatPublish", Helpers.JsonSerialise(addressStatDto));
        }
    }
}