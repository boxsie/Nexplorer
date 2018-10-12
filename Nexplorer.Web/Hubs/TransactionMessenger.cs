using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Web.Hubs
{
    public class TransactionMessenger
    {
        private readonly RedisCommand _redisCommand;
        private readonly IHubContext<TransactionHub> _txContext;

        public TransactionMessenger(RedisCommand redisCommand, IHubContext<TransactionHub> txContext)
        {
            _redisCommand = redisCommand;
            _txContext = txContext;

            _redisCommand.Subscribe<TransactionLiteDto>(Settings.Redis.NewTransactionPubSub, OnNewTransactionAsync);
        }

        private Task OnNewTransactionAsync(TransactionLiteDto newTx)
        {
            return _txContext.Clients.All.SendAsync("NewTxPubSub", newTx);
        }
    }
}