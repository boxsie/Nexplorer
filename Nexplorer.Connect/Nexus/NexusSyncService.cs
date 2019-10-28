using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Nexplorer.Core;
using Nexplorer.Data;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Connect.Nexus
{
    public class NexusSyncService : ScheduledService
    {
        private readonly INexusHubService _hubService;
        private readonly IBlockDb _blockDb;

        private CancellationToken _token;
        private bool _catchingUp;

        public NexusSyncService(INexusHubService hubService, IBlockDb blockDb, ILogger<NexusSyncService> logger) 
            : base(TimeSpan.FromSeconds(10), logger)
        {
            _hubService = hubService;
            _blockDb = blockDb;
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
            var lastNodeHeight = await _hubService.GetHeightAsync(_token);

            Logger.LogInformation($"Nexus node height:{lastNodeHeight} | Nexplorer DB height:{lastDbHeight}");

            if (lastDbHeight >= lastNodeHeight)
                return;

            while (lastDbHeight < lastNodeHeight)
            {
                var response = await _hubService.GetBlocksAsync(lastDbHeight + 1, 1000, _token);

                var blocks = response == null 
                    ? await GetBlocksHandleErrorsAsync(lastDbHeight + 1, 1000) 
                    : response.ToList();

                if (!blocks.Any())
                    return;

                Logger.LogInformation($"Inserting {blocks.Count} blocks");

                await _blockDb.CreateManyAsync(blocks);

                var first = blocks.First().Height;
                var last = blocks.Last().Height;

                Logger.LogInformation($"Finished inserting {(first == last ? $"block {first}" : $"blocks {blocks.First().Height} - {blocks.Last().Height}")}");

                lastDbHeight = (await _blockDb.GetHighestAsync())?.Height ?? 0;

                lastNodeHeight = await _hubService.GetHeightAsync(_token);

                Logger.LogInformation($"Nexus node height:{lastNodeHeight} | Nexplorer DB height:{lastDbHeight}");

                await Task.Delay(TimeSpan.FromSeconds(1), _token);
            }

            _catchingUp = false;
        }

        private Task OnNewHeightAsync(int newHeight)
        {
            if (!_catchingUp)
                Logger.LogInformation($"New block {newHeight:N0} reported");
            
            return Task.CompletedTask;
        }

        private async Task<List<Block>> GetBlocksHandleErrorsAsync(int start, int count)
        {
            var blocks = new List<Block>();

            for (var i = start; i < start + count; i++)
            {
                var block = await _hubService.GetBlockAsync(i, _token);

                if (block == null)
                {
                    Logger.LogError($"Found null block at height {i}");
                    block = new Block { Height = i };
                }

                blocks.Add(block);
            }

            return blocks;
        }
    }
}