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
        private readonly ILogger<NewBlockService> _logger;

        public NewBlockService(ILedgerService ledgerService, IHubContext<NexplorerHub, INexplorer> nexplorerHub, 
            Globals globals, ILogger<NewBlockService> logger) : base(TimeSpan.FromSeconds(10))
        {
            _ledgerService = ledgerService;
            _nexplorerHub = nexplorerHub;
            _globals = globals;
            _logger = logger;
        }

        public override async Task Execute()
        {
            var height = await _ledgerService.GetHeightAsync();

            if (!height.HasValue)
                return;

            if (_globals.LastKnownHeight == 0)
            {
                _globals.LastKnownHeight = height.Value;
                return;
            }

            while (_globals.LastKnownHeight < height)
            {
                _globals.LastKnownHeight++;

                _logger.LogInformation($"New block found - publishing height {_globals.LastKnownHeight:N0}");

                await _nexplorerHub.Clients.All.PublishNewHeight(_globals.LastKnownHeight);
            }
        }
    }
}