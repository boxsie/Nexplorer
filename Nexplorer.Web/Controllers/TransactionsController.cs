using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nexplorer.Config;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Models;
using Nexplorer.Web.Models;

namespace Nexplorer.Web.Controllers
{
    public class TransactionsController : WebControllerBase
    {
        private readonly BlockQuery _blockQuery;
        private readonly TransactionQuery _transactionQuery;

        public TransactionsController(BlockQuery blockQuery, TransactionQuery transactionQuery)
        {
            _blockQuery = blockQuery;
            _transactionQuery = transactionQuery;
        }
        
        public async Task<IActionResult> Index()
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
                Transaction = new TransactionModel(tx),
                BlockHash = blockHash,
                MaxConfirmations = Settings.App.MaxConfirmations
            };

            return View(viewModel);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetRecentTransactions(int? start, int? count)
        {
            var txs = (await _transactionQuery.GetNewTransactionCacheAsync()).ToList();

            return Ok(txs.Skip(start ?? 0).Take(count ?? txs.Count));
        }
    }
}
