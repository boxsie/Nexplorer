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
    public class BlockSync : SyncJob
    {
        private readonly IBlockCache _blockCache;
        private readonly NexusQuery _nexusQuery;
        private readonly BlockQuery _blockQuery;
        private readonly RollingCountPublisher _countPublisher;
        private readonly NexusDb _nexusDb;
        private readonly IMapper _mapper;

        public BlockSync(IBlockCache blockCache, NexusQuery nexusQuery, BlockQuery blockQuery, RollingCountPublisher countPublisher, 
            NexusDb nexusDb, ILogger<BlockSync> logger, IMapper mapper) 
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
                await SyncBlocks(syncBlocks);

                await _blockCache.RemoveAllBelowAsync(await _blockQuery.GetLastSyncedHeightAsync());
            }

            await _countPublisher.PublishRollingCountAsync();

            return "Block sync is up to date";
        }

        private async Task SyncBlocks(List<BlockDto> blockDtos)
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

            await blockDtos.InsertBlocksAsync();

            var orphans = orphanBlocks.Select(x => _mapper.Map<OrphanBlock>(x));
            await _nexusDb.OrphanBlocks.AddRangeAsync(orphans);
            await _nexusDb.SaveChangesAsync();
        }

        //private async Task UpdateAddressAggregates(IEnumerable<Block> blocks)
        //{
        //    foreach (var block in blocks)
        //    {
        //        foreach (var tx in block.Transactions)
        //        {
        //            foreach (var txIn in tx.Inputs)
        //                await _addressUpdateCommand.UpdateAsync(_nexusDb, txIn.Address.AddressId, TransactionType.Input, txIn.Amount, txIn.Transaction.Block);

        //            foreach (var txOut in tx.Outputs)
        //                await _addressUpdateCommand.UpdateAsync(_nexusDb, txOut.Address.AddressId, TransactionType.Output, txOut.Amount, txOut.Transaction.Block);
        //        }
        //    }
        //}
    }
}