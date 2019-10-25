using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nexplorer.Core;
using Nexplorer.Nexus;
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
            var msg = await _ledgerService.GetMiningInfoAsync();

            if (!msg.HasError)
            {
                await Clients.Caller.PublishNewHeight(msg.Result.Blocks);
                await base.OnConnectedAsync();
            }
        }
        
        public Task<NexusResponse<int>> GetHeight()
        {
            return _ledgerService.GetHeightAsync();
        }

        public Task<NexusResponse<Block>> GetBlock(int height)
        {
            return _ledgerService.GetBlockAsync(height);
        }

        public Task<NexusResponse<IEnumerable<Block>>> GetBlocks(int start, int count)
        {
            return _ledgerService.GetBlocksAsync(start, count);
        }
    }
}