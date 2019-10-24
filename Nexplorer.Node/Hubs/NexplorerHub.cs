using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nexplorer.Core;
using Nexplorer.Nexus.Ledger;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Node.Hubs
{
    public class NexplorerHub : Hub<INexplorer>
    {
        private readonly ILedgerService _ledgerService;

        public NexplorerHub(ILedgerService ledgerService)
        {
            _ledgerService = ledgerService;
        }

        public override async Task OnConnectedAsync()
        {
            var height = (await _ledgerService.GetMiningInfoAsync()).Blocks;
            await Clients.Caller.PublishNewHeight(height);
            await base.OnConnectedAsync();
        }
        
        public async Task<int?> GetHeight()
        {
            return (await _ledgerService.GetMiningInfoAsync()).Blocks;
        }

        public Task<Block> GetBlock(int height)
        {
            return _ledgerService.GetBlockAsync(height);
        }

        public Task<IEnumerable<Block>> GetBlocks(int start, int count)
        {
            return _ledgerService.GetBlocksAsync(start, count);
        }
    }
}