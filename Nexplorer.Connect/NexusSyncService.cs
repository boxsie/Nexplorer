using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexplorer.Core;
using Nexplorer.Data;

namespace Nexplorer.Connect
{
    public class NexusSyncService : ScheduledService
    {
        private readonly INexusHubService _hubService;
        private readonly IBlockDb _blockDb;
        private readonly ILogger<NexusSyncService> _logger;

        private CancellationToken _token;
        private bool _catchingUp;

        public NexusSyncService(INexusHubService hubService, IBlockDb blockDb, ILogger<NexusSyncService> logger) 
            : base(TimeSpan.FromSeconds(10))
        {
            _hubService = hubService;
            _blockDb = blockDb;
            _logger = logger;
            _catchingUp = false;
        }

        public override async Task Execute()
        {
            await _hubService.ConnectAsync(_token);

            if (_hubService.State == HubConnectionState.Connected)
                await Catchup();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _token = cancellationToken;

            await _hubService.RegisterAsync<int>("PublishNewHeight", OnNewHeightAsync);

            await base.StartAsync(_token);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return _hubService.DisposeAsync();
        }

        private async Task Catchup()
        {
            _catchingUp = true;

            var lastDbHeight = (await _blockDb.GetHighestAsync())?.Height ?? 0;
            var lastNodeHeight = await _hubService.GetHeightAsync(_token) ?? 0;

            _logger.LogInformation($"Nexus node height:{lastNodeHeight} | Nexplorer DB height:{lastDbHeight}");

            if (lastDbHeight >= lastNodeHeight)
                return;

            while (lastDbHeight < lastNodeHeight)
            {
                var response = await _hubService.GetBlocksAsync(lastDbHeight + 1, 1000, _token);
                
                if (response == null)
                    return;

                var blocks = response.ToList();

                if (!blocks.Any())
                    return;

                _logger.LogInformation($"Inserting {blocks.Count} blocks");

                await _blockDb.CreateManyAsync(blocks);

                var first = blocks.First().Height;
                var last = blocks.Last().Height;

                _logger.LogInformation($"Finished inserting {(first == last ? $"block {first}" : $"blocks {blocks.First().Height} - {blocks.Last().Height}")}");

                lastDbHeight = (await _blockDb.GetHighestAsync())?.Height ?? 0;

                lastNodeHeight = await _hubService.GetHeightAsync(_token) ?? 0;

                _logger.LogInformation($"Nexus node height:{lastNodeHeight} | Nexplorer DB height:{lastDbHeight}");
            }

            _catchingUp = false;
        }

        private Task OnNewHeightAsync(int newHeight)
        {
            if (!_catchingUp)
                _logger.LogInformation($"New block {newHeight:N0} reported");
            
            return Task.CompletedTask;
        }
    }
}