using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Web.Hubs
{
    public class AddressHub : Hub
    {
        private readonly RedisCommand _redisCommand;

        public AddressHub(RedisCommand redisCommand)
        {
            _redisCommand = redisCommand;
        }

        public async Task<string> GetLatestAddressStats()
        {
            return Helpers.JsonSerialise(await _redisCommand.GetAsync<AddressStatDto>(Settings.Redis.AddressStatPubSub));
        }
    }
}