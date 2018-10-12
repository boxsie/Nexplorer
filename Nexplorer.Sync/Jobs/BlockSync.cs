using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Data.Cache.Block;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Data.Command;
using Nexplorer.Data.Publish;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Sync.Core;
using Quartz;

namespace Nexplorer.Sync.Jobs
{
    public class BlockSync : SyncJob
    {
        private readonly IBlockCache _blockCache;
        private readonly NexusQuery _nexusQuery;
        private readonly BlockQuery _blockQuery;
        private readonly BlockAddCommand _blockAdd;
        private readonly RollingCountPublisher _countPublisher;

        public BlockSync(IBlockCache blockCache, NexusQuery nexusQuery, BlockQuery blockQuery, BlockAddCommand blockAdd, 
            RollingCountPublisher countPublisher, ILogger<BlockSync> logger) 
            : base(logger, 30)
        {
            _blockCache = blockCache;
            _nexusQuery = nexusQuery;
            _blockQuery = blockQuery;
            _blockAdd = blockAdd;
            _countPublisher = countPublisher;
        }

        protected override async Task<string> ExecuteAsync()
        {
            var cache = await _blockCache.GetBlockCacheAsync();

            if (cache == null)
                return "Cache is null!";
            
            var syncBlocks = cache.Skip(Settings.App.BlockCacheCount).Reverse().ToList();

            if (syncBlocks.Count > 0)
            {
                await SyncBlocks(syncBlocks);

                await _blockCache.RemoveAllBelowAsync(await _blockQuery.GetLastSyncedHeightAsync());
            }

            await _countPublisher.PublishRollingCountAsync();

            return "Block sync is up to date";
        }

        private async Task SyncBlocks(IEnumerable<BlockDto> blockDtos)
        {
            var syncBlocks = new List<BlockDto>();
            var orphanBlocks = new List<BlockDto>();

            foreach (var blockDto in blockDtos)
            {
                var validateBlock = await _nexusQuery.GetBlockAsync(blockDto.Height, false);

                BlockDto finalBlockDto;

                if (validateBlock.Hash != blockDto.Hash)
                {
                    Logger.LogInformation($"Orphan block found {blockDto.Hash}");
                    
                    finalBlockDto = await _nexusQuery.GetBlockAsync(validateBlock.Hash, true);

                    orphanBlocks.Add(blockDto);
                }
                else
                    finalBlockDto = blockDto;

                syncBlocks.Add(finalBlockDto);

                Logger.LogInformation($"Syncing block {finalBlockDto.Height}");
            }

            await _blockAdd.AddBlocksAsync(syncBlocks);
            await _blockAdd.AddOrphansAsync(orphanBlocks);
        }
    }
}