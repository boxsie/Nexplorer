using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Nexplorer.Core;
using Nexplorer.Nexus.Ledger;
using Nexplorer.Node.Hubs;
using Nexplorer.Node.Settings;

namespace Nexplorer.Node.Services
{
    public class NewBlockService : ScheduledService
    {
        private readonly ILedgerService _ledgerService;
        private readonly IHubContext<NexplorerHub, INexplorer> _nexplorerHub;
        private readonly Globals _globals;

        public NewBlockService(ILedgerService ledgerService, IHubContext<NexplorerHub, INexplorer> nexplorerHub, 
            Globals globals, ILogger<NewBlockService> logger) 
            : base(TimeSpan.FromSeconds(10), logger)
        {
            _ledgerService = ledgerService;
            _nexplorerHub = nexplorerHub;
            _globals = globals;
        }

        public override async Task Execute()
        {
            var msg = await _ledgerService.GetHeightAsync();

            if (msg.HasError)
                return;

            var height = msg.Result;

            if (_globals.LastKnownHeight == 0)
            {
                _globals.LastKnownHeight = height;
                return;
            }

            while (_globals.LastKnownHeight < height)
            {
                _globals.LastKnownHeight++;

                Logger.LogInformation($"New block found - publishing height {_globals.LastKnownHeight:N0}");

                await _nexplorerHub.Clients.All.PublishNewHeight(_globals.LastKnownHeight);
            }
        }
    }
}