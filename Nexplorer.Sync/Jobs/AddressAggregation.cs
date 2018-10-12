using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Data.Command;
using Nexplorer.Data.Context;
using Nexplorer.Domain.Enums;
using Nexplorer.Sync.Core;
using Quartz;

namespace Nexplorer.Sync.Jobs
{
    public class AddressAggregation : SyncJob
    {
        private readonly NexusDb _nexusDb;
        private readonly AddressAggregateUpdateCommand _addressAggregateUpdate;

        public AddressAggregation(ILogger<AddressAggregation> logger, NexusDb nexusDb, AddressAggregateUpdateCommand addressAggregateUpdate) : base(logger, 5)
        {
            _addressAggregateUpdate = addressAggregateUpdate;
            _nexusDb = nexusDb;
        }

        protected override async Task<string> ExecuteAsync()
        {
            var lastBlockHeight = await _nexusDb.AddressAggregates.AnyAsync()
                ? await _nexusDb.AddressAggregates.MaxAsync(x => x.LastBlock.Height)
                : 0;

            var dbHeight = await _nexusDb.Blocks
                .OrderBy(x => x.Height)
                .Select(x => x.Height)
                .LastOrDefaultAsync();

            if (lastBlockHeight < dbHeight)
            {
                var nextBlockHeight = lastBlockHeight + 1;

                var updateCount = 0;

                var bulkSaveCount = Settings.App.BulkSaveCount * 10;

                var lastHeight = dbHeight - nextBlockHeight > bulkSaveCount
                    ? nextBlockHeight + bulkSaveCount
                    : dbHeight;

                Logger.LogInformation($"Adding address aggregate data from block { nextBlockHeight } -> { lastHeight }");

                for (var i = nextBlockHeight; i < nextBlockHeight + bulkSaveCount; i++)
                {
                    var height = i;

                    if (height > dbHeight)
                        break;

                    var nextBlock = await _nexusDb.Blocks
                        .Include(x => x.Transactions)
                        .ThenInclude(x => x.Inputs)
                        .ThenInclude(x => x.Address)
                        .Include(x => x.Transactions)
                        .ThenInclude(x => x.Outputs)
                        .ThenInclude(x => x.Address)
                        .FirstOrDefaultAsync(x => x.Height == height);

                    if (nextBlock == null)
                        break;

                    foreach (var transaction in nextBlock.Transactions)
                    {
                        foreach (var transactionInput in transaction.Inputs)
                            await _addressAggregateUpdate.UpdateAsync(_nexusDb, transactionInput.Address.AddressId, TransactionType.Input, transactionInput.Amount, nextBlock);

                        foreach (var transactionOutput in transaction.Outputs)
                            await _addressAggregateUpdate.UpdateAsync(_nexusDb, transactionOutput.Address.AddressId, TransactionType.Output, transactionOutput.Amount, nextBlock);
                    }

                    updateCount++;
                }
                
                await _nexusDb.SaveChangesAsync();

                _addressAggregateUpdate.Reset();

                return $"{updateCount} address aggregates updated";
            }

            return null;
        }
    }
}