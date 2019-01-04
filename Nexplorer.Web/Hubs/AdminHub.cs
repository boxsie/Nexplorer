using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Web.Hubs
{
    public class AdminHub : Hub
    {
        private readonly RedisCommand _redisCommand;
        private readonly AdminMessenger _messenger;

        public AdminHub(RedisCommand redisCommand, AdminMessenger messenger)
        {
            _redisCommand = redisCommand;
            _messenger = messenger;
        }

        public List<string> GetLatestSyncOutputs()
        {
            return _messenger.RecentSyncOutputs;
        }
    }
}