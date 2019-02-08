using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nexplorer.Config;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Criteria;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;
using Nexplorer.Domain.Models;
using Nexplorer.Web.Dtos;
using Nexplorer.Web.Models;

namespace Nexplorer.Web.Controllers
{
    public class TransactionsController : WebControllerBase
    {
        private readonly BlockQuery _blockQuery;
        private readonly TransactionQuery _transactionQuery;

        private const int MaxTxsPerFilterPage = 100;

        public TransactionsController(BlockQuery blockQuery, TransactionQuery transactionQuery)
        {
            _blockQuery = blockQuery;
            _transactionQuery = transactionQuery;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Transaction(string txHash)
        {
            if (txHash == null)
                return RedirectToAction("index");

            var tx = await _transactionQuery.GetTransaction(txHash);

            if (tx == null)
                return RedirectToAction("index");

            var blockHash = await _blockQuery.GetBlockHashAsync(tx.BlockHeight);

            var viewModel = new TransactionViewModel
            {
                Transaction = tx,
                BlockHash = blockHash
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetTransactions(DataTablePostModel<TransactionFilterCriteria> model)
        {
            var criteria = model.Filter != "custom"
                ? GetCriteria(model.Filter)
                : model.FilterCriteria;

            var count = model.Length > MaxTxsPerFilterPage
                ? MaxTxsPerFilterPage
                : model.Length;
            
            var data = await _transactionQuery.GetTransactionsFilteredAsync(criteria, model.Start, count, true, 1000);
            
            var response = new
            {
                Draw = model.Draw,
                RecordsTotal = 0,
                RecordsFiltered = data.ResultCount,
                Data = data.Results
            };

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentTransactions(int start, int count)
        {
            return Ok(await _transactionQuery.GetTransactionsFilteredAsync(new TransactionFilterCriteria { OrderBy = OrderTransactionsBy.MostRecent }, start, count, false));
        }

        private TransactionFilterCriteria GetCriteria(string filter)
        {
            switch (filter)
            {
                case "latest":
                    return new TransactionFilterCriteria { OrderBy = OrderTransactionsBy.MostRecent };
                case "user":
                    return new TransactionFilterCriteria { OrderBy = OrderTransactionsBy.MostRecent, TxType = TransactionType.User };
                default:
                    return new TransactionFilterCriteria { OrderBy = OrderTransactionsBy.MostRecent };
            }
        }
    }
}
