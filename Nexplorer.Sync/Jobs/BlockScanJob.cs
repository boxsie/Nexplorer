using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Data.Cache.Block;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Data.Publish;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Sync.Core;

namespace Nexplorer.Sync.Jobs
{
    public class BlockScanJob : SyncJob
    {
        private readonly IBlockCache _blockCache;
        private readonly NexusQuery _nexusQuery;
        private readonly BlockQuery _blockQuery;
        private readonly LatestBlockPublisher _blockPublisher;

        public BlockScanJob(IBlockCache blockCache, NexusQuery nexusQuery, BlockQuery blockQuery, 
            LatestBlockPublisher blockPublisher, ILogger<BlockScanJob> logger) 
            : base(logger, 1)
        {
            _blockCache = blockCache;
            _nexusQuery = nexusQuery;
            _blockQuery = blockQuery;
            _blockPublisher = blockPublisher;
        }

        protected override async Task<string> ExecuteAsync()
        {
            var nextHeight = await _blockCache.GetBlockCacheHeightAsync();

            if (nextHeight == 0)
                nextHeight = await _blockQuery.GetLastHeightAsync();

            nextHeight += 1;

            var nextBlockDto = await _nexusQuery.GetBlockAsync(nextHeight, true);

            if (nextBlockDto == null)
                return null;

            var newBlocks = new List<BlockDto>();

            while (nextBlockDto != null)
            {
                Logger.LogInformation($"Found new block {nextBlockDto.Height}");

                if (!await _blockCache.BlockExistsAsync(nextBlockDto.Height))
                    newBlocks.Add(nextBlockDto);
                else
                    Logger.LogInformation($"Block {nextBlockDto.Height} is already in the cache");

                nextBlockDto = await _nexusQuery.GetBlockAsync(nextBlockDto.Height + 1, true);
            }

            if (newBlocks.Count <= 0)
                return null;

            foreach (var newBlock in newBlocks)
                await _blockCache.AddAsync(newBlock);

            await _blockCache.SaveAsync();

            foreach (var newBlock in newBlocks)
                await _blockPublisher.PublishLatestDataAsync(newBlock);

            return newBlocks.Count > 1 
                ? $"Published blocks {string.Join(", ", newBlocks)}" 
                : null;
        }
    }
}