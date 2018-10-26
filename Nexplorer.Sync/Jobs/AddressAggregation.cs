//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Nexplorer.Config;
//using Nexplorer.Data.Command;
//using Nexplorer.Data.Context;
//using Nexplorer.Domain.Enums;
//using Nexplorer.Sync.Core;
//using Quartz;

//namespace Nexplorer.Sync.Jobs
//{
//    public class AddressAggregation : SyncJob
//    {
//        private readonly NexusDb _nexusDb;

//        public AddressAggregation(ILogger<AddressAggregation> logger, NexusDb nexusDb) : base(logger, 5)
//        {
//            _nexusDb = nexusDb;
//        }

//        protected override async Task<string> ExecuteAsync()
//        {
//            var lastBlockHeight = await _nexusDb.AddressAggregates.AnyAsync()
//                ? await _nexusDb.AddressAggregates.MaxAsync(x => x.LastBlockHeight)
//                : 0;

//            var dbHeight = await _nexusDb.Blocks
//                .OrderBy(x => x.Height)
//                .Select(x => x.Height)
//                .LastOrDefaultAsync();

//            if (lastBlockHeight >= dbHeight)
//                return null;

//            var nextBlockHeight = lastBlockHeight + 1;

//            var bulkSaveCount = Settings.App.BulkSaveCount * 10;

//            var lastHeight = dbHeight - nextBlockHeight > bulkSaveCount
//                ? nextBlockHeight + bulkSaveCount
//                : dbHeight;

//            Logger.LogInformation($"Adding address aggregate data from block { nextBlockHeight } -> { lastHeight }");

//            await AddressAggregator.AggregateAddresses(nextBlockHeight, nextBlockHeight + bulkSaveCount);

//            return $"Address aggregates updated";

//        }
//    }
//}