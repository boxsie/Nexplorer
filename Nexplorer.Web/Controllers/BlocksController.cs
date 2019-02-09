using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexplorer.Data.Command;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Criteria;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;
using Nexplorer.Domain.Models;
using Nexplorer.Web.Dtos;
using Nexplorer.Web.Models;

namespace Nexplorer.Web.Controllers
{
    public class BlocksController : WebControllerBase
    {
        private readonly BlockQuery _blockQuery;
        private readonly BlockCacheCommand _cacheCommand;

        private const int MaxBlocksPerFilterPage = 100;

        public BlocksController(BlockQuery blockQuery, BlockCacheCommand cacheCommand)
        {
            _blockQuery = blockQuery;
            _cacheCommand = cacheCommand;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Block(string blockId)
        {
            if (blockId == null)
                return RedirectToAction("index");

            var usingHeight = int.TryParse(blockId, out var height);
            
            var block = usingHeight
                ? await _blockQuery.GetBlockAsync(height, true) 
                : await _blockQuery.GetBlockAsync(blockId, true);

            if (block == null)
                return RedirectToAction("index");

            var channelHeight = await _blockQuery.GetChannelHeightAsync((BlockChannels)block.Channel, block.Height);
            var confirmations = await _blockQuery.GetLastHeightAsync() - (block.Height - 1);

            var viewModel = new BlockViewModel(block, channelHeight, confirmations);

            return View(viewModel);
        }

        public async Task<IActionResult> Latest()
        {
            return Redirect($"/blocks/{await _blockQuery.GetLastHeightAsync()}");
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentBlocks()
        {
            return Ok(await _cacheCommand.GetAsync());
        }

        [HttpPost]
        public async Task<IActionResult> GetBlocks(DataTablePostModel<BlockFilterCriteria> model)
        {
            var criteria = GetCriteria(model.Filter) ?? model.FilterCriteria;

            var count = model.Length > MaxBlocksPerFilterPage
                ? MaxBlocksPerFilterPage
                : model.Length;

            var data = await _blockQuery.GetBlocksFilteredAsync(criteria, model.Start, count, true, 1000);

            var response = new
            {
                Draw = model.Draw,
                RecordsTotal = 0,
                RecordsFiltered = data.ResultCount,
                Data = data.Results
            };

            return Ok(response);
        }

        private BlockFilterCriteria GetCriteria(string filter)
        {
            switch (filter)
            {
                case "latest":
                    return new BlockFilterCriteria { OrderBy = OrderBlocksBy.Highest };
                default:
                    return null;
            }
        }
    }
}
