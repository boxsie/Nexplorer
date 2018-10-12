using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Web.Hubs
{
    public class BlockMessenger
    {
        private readonly RedisCommand _redisCommand;
        private readonly IHubContext<BlockHub> _blockContext;

        public BlockMessenger(RedisCommand redisCommand, IHubContext<BlockHub> blockContext)
        {
            _redisCommand = redisCommand;
            _blockContext = blockContext;

            _redisCommand.Subscribe<BlockLiteDto>(Settings.Redis.NewBlockPubSub, OnNewBlockAsync);
        }

        private Task OnNewBlockAsync(BlockLiteDto newBlock)
        {
            return _blockContext.Clients.All.SendAsync("NewBlockPubSub", newBlock);
        }
    }
}