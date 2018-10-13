//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Nexplorer.Config;
//using Nexplorer.Data.Command;
//using Nexplorer.Data.Context;
//using Nexplorer.Data.Map;
//using Nexplorer.Data.Query;
//using Nexplorer.Domain.Entity.Blockchain;
//using Nexplorer.Domain.Enums;
//using Nexplorer.Sync.Core;
//using Quartz;

//namespace Nexplorer.Sync.Jobs
//{
//    public class TransactionRefresh : SyncJob
//    {
//        private readonly NexusDb _nexusDb;
//        private readonly NexusQuery _nexusQuery;
//        private readonly AddressAggregateUpdateCommand _addressAggregateUpdate;
//        private readonly TransactionInputOutputMapper _txInOutMapper;

//        public TransactionRefresh(ILogger<TransactionRefresh> logger, NexusDb nexusDb, NexusQuery nexusQuery, 
//            AddressAggregateUpdateCommand addressAggregateUpdate, TransactionInputOutputMapper txInOutMapper)
//            : base(logger, 120)
//        {
//            _nexusDb = nexusDb;
//            _nexusQuery = nexusQuery;
//            _addressAggregateUpdate = addressAggregateUpdate;
//            _txInOutMapper = txInOutMapper;
//        }

//        protected override async Task<string> ExecuteAsync()
//        {
//            var missingTxs = await _nexusDb
//                .Transactions.Include(x => x.Block)
//                .Where(x => x.Amount == 0)
//                .Take(Settings.App.BulkSaveCount)
//                .ToListAsync();

//            if (missingTxs.Count == 0)
//                return "No transactions to refresh";

//            foreach (var transaction in missingTxs)
//            {
//                var dto = await _nexusQuery.GetTransactionAsync(transaction.Hash, transaction.Block.Height);

//                if (dto == null || dto.Amount == 0)
//                    continue;

//                transaction.Amount = dto.Amount;
//                transaction.Timestamp = dto.Timestamp;

//                transaction.Inputs = await _txInOutMapper.MapTransactionInputOutput<TransactionInput>(_nexusDb, dto.Inputs, transaction.Block, transaction);
//                transaction.Outputs = await _txInOutMapper.MapTransactionInputOutput<TransactionOutput>(_nexusDb, dto.Outputs, transaction.Block, transaction);
//            }

//            await _nexusDb.SaveChangesAsync();

//            foreach (var transaction in missingTxs.Where(x => x.Amount > 0))
//            {
//                foreach (var txIn in transaction.Inputs)
//                    await _addressAggregateUpdate.UpdateAsync(_nexusDb, txIn.Address.AddressId, TransactionType.Input, txIn.Amount, txIn.Transaction.Block);

//                foreach (var txOut in transaction.Outputs)
//                    await _addressAggregateUpdate.UpdateAsync(_nexusDb, txOut.Address.AddressId, TransactionType.Output, txOut.Amount, txOut.Transaction.Block);
//            }

//            _txInOutMapper.Reset();
//            _addressAggregateUpdate.Reset();

//            return $"Refreshed {missingTxs.Count(x => x.Amount > 0)} transactions";
//        }
//    }
//}