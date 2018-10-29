using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Data.Cache.Block;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Data.Command;
using Nexplorer.Data.Context;
using Nexplorer.Data.Publish;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Domain.Entity.Orphan;
using Nexplorer.Domain.Enums;
using Nexplorer.Sync.Core;
using Quartz;

namespace Nexplorer.Sync.Jobs
{
    public class BlockSyncJob : SyncJob
    {
        private readonly IBlockCache _blockCache;
        private readonly NexusQuery _nexusQuery;
        private readonly BlockQuery _blockQuery;
        private readonly RollingCountPublisher _countPublisher;
        private readonly NexusDb _nexusDb;
        private readonly IMapper _mapper;

        public BlockSyncJob(IBlockCache blockCache, NexusQuery nexusQuery, BlockQuery blockQuery, RollingCountPublisher countPublisher, 
            NexusDb nexusDb, ILogger<BlockSyncJob> logger, IMapper mapper) 
            : base(logger, 30)
        {
            _blockCache = blockCache;
            _nexusQuery = nexusQuery;
            _blockQuery = blockQuery;
            _countPublisher = countPublisher;
            _nexusDb = nexusDb;
            _mapper = mapper;
        }

        protected override async Task<string> ExecuteAsync()
        {
            var cache = await _blockCache.GetBlockCacheAsync();

            if (cache == null)
                return "Cache is null!";
            
            var syncBlocks = cache.Skip(Settings.App.BlockCacheCount).Reverse().ToList();

            if (syncBlocks.Count > 0)
            {
                var newBlockDtos = new List<BlockDto>();
                var orphanBlocks = new List<BlockDto>();

                var lastSyncedBlockHash = await _blockQuery.GetLastSyncedBlockHashAsync();
                var nextBlock = await _nexusQuery.GetBlockAsync(lastSyncedBlockHash, false);
                var nextBlockHash = nextBlock.NextBlockHash;

                for (var i = 0; i < syncBlocks.Count; i++)
                {
                    nextBlock = await _nexusQuery.GetBlockAsync(nextBlockHash, true);

                    newBlockDtos.Add(nextBlock);

                    nextBlockHash = nextBlock.NextBlockHash;
                }

                var newBlocks = await newBlockDtos.InsertBlocksAsync();

                using (var addAgg = new AddressAggregator())
                    await addAgg.AggregateAddresses(newBlocks);

                await _nexusDb.OrphanBlocks.AddRangeAsync(syncBlocks
                    .Where(x => newBlocks.All(y => y.Hash != x.Hash))
                    .Select(x => _mapper.Map<OrphanBlock>(x)));

                await _nexusDb.SaveChangesAsync();

                await _blockCache.RemoveAllBelowAsync(await _blockQuery.GetLastSyncedHeightAsync());
            }

            await _countPublisher.PublishRollingCountAsync();

            return "Block sync is up to date";
        }
    }
}