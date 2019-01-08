using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Criteria;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;
using Nexplorer.Domain.Models;
using Nexplorer.Web.Models;

namespace Nexplorer.Web.Controllers
{
    public class BlocksController : WebControllerBase
    {
        private readonly BlockQuery _blockQuery;

        public BlocksController(BlockQuery blockQuery)
        {
            _blockQuery = blockQuery;
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

            var channelHeight = await _blockQuery.GetChannelHeight((BlockChannels)block.Channel, block.Height);
            var confirmations = await _blockQuery.GetLastHeightAsync() - (block.Height - 1);

            var viewModel = new BlockViewModel(block, channelHeight, confirmations);

            return View(viewModel);
        }

        public async Task<IActionResult> Latest()
        {
            return Redirect($"/blocks/{await _blockQuery.GetLastHeightAsync()}");
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentBlocks(int start, int count)
        {
            return Ok(await _blockQuery.GetBlocksFilteredAsync(new BlockFilterCriteria { OrderBy = OrderBlocksBy.Highest }, start, count, false));
        }
    }
}
