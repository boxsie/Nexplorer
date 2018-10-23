//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Nexplorer.Config;
//using Nexplorer.Data.Cache;
//using Nexplorer.Data.Cache.Block;
//using Nexplorer.Data.Cache.Services;
//using Nexplorer.Data.Query;
//using Nexplorer.Sync.Core;
//using Quartz;

//namespace Nexplorer.Sync.Jobs
//{
//    public class BlockCacheUpdate : SyncJob
//    {
//        private readonly BlockCache _blockCache;
//        private readonly NexusQuery _nexusQuery;
//        private readonly BlockQuery _blockQuery;

//        public BlockCacheUpdate(BlockCache blockCache, NexusQuery nexusQuery, BlockQuery blockQuery, ILogger<BlockCacheUpdate> logger) 
//            : base(logger, 120)
//        {
//            _blockCache = blockCache;
//            _nexusQuery = nexusQuery;
//            _blockQuery = blockQuery;
//        }

//        protected override async Task<string> ExecuteAsync()
//        {
//            var cache = await _blockCache.GetBlockCacheAsync();

//            if (cache == null)
//                return "Cache is null!";

//            var updates = new List<BlockCacheTransaction>();

//            foreach (var block in cache)
//            {
//                foreach (var tx in block.Transactions)
//                {
//                    var latestTx = await _nexusQuery.GetTransactionAsync(tx.Hash, block.Height);

//                    if (latestTx == null || tx.Confirmations == latestTx.Confirmations)
//                        continue;

//                    updates.Add(new BlockCacheTransaction
//                    {
//                        Height = block.Height,
//                        TxHash = tx.Hash,
//                        TransactionUpdate = new BlockCacheTransactionUpdate
//                        {
//                            Confirmations = latestTx.Confirmations
//                        }
//                    });
//                }
//            }

//            await _blockCache.UpdateTransactionsAsync(updates);

//            return $"{updates.Count} transactions updated from {cache.Count} blocks";
//        }
//    }
//}