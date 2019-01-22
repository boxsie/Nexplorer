using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Services;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Web.Hubs
{
    public class HomeMessenger
    {
        private readonly RedisCommand _redisCommand;
        private readonly IHubContext<HomeHub> _homeContext;

        public HomeMessenger(RedisCommand redisCommand, IHubContext<HomeHub> homeContext)
        {
            _redisCommand = redisCommand;
            _homeContext = homeContext;

            _redisCommand.Subscribe<BlockLiteDto>(Settings.Redis.NewBlockPubSub, SendLatestBlockDataAsync);
            _redisCommand.Subscribe<TransactionLiteDto>(Settings.Redis.NewTransactionPubSub, OnNewTransactionAsync);
        }

        private async Task SendLatestBlockDataAsync(BlockLiteDto block)
        {
            await _homeContext.Clients.All.SendAsync("NewBlockPubSub", Helpers.JsonSerialise(block));
        }

        private Task OnNewTransactionAsync(TransactionLiteDto tx)
        {
            return _homeContext.Clients.All.SendAsync("NewTxPubSub", Helpers.JsonSerialise(tx));
        }
    }
}