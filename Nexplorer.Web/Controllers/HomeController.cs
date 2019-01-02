using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;
using Nexplorer.Web.Cookies;
using Nexplorer.Web.Extensions;
using Nexplorer.Web.Models;

namespace Nexplorer.Web.Controllers
{
    public class HomeController : WebControllerBase
    {
        private readonly BlockQuery _blockQuery;
        private readonly TransactionQuery _txQuery;
        private readonly RedisCommand _redisCommand;
        private readonly StatQuery _statQuery;

        public HomeController(BlockQuery blockQuery, TransactionQuery txQuery, RedisCommand redisCommand, StatQuery statQuery)
        {
            _blockQuery = blockQuery;
            _txQuery = txQuery;
            _redisCommand = redisCommand;
            _statQuery = statQuery;
        }

        public async Task<IActionResult> Index()
        {
            var channelStats = await _statQuery.GetChannelStatsAsync();

            return View(new HomeViewModel
            { 
                LastBlock = await _blockQuery.GetLastBlockAsync(),
                LastPosDifficulty = channelStats?.FirstOrDefault(x => x.Channel == BlockChannels.PoS.ToString())?.Difficulty ?? 0,
                LastPrimeDifficulty = channelStats?.FirstOrDefault(x => x.Channel == BlockChannels.Prime.ToString())?.Difficulty ?? 0,
                LastHashDifficulty = channelStats?.FirstOrDefault(x => x.Channel == BlockChannels.Hash.ToString())?.Difficulty ?? 0
            });
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Cookie()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DismissCookieWarning()
        {
            var cookie = Request.GetCookie<UserSettingsCookieData>();

            cookie.DismissedCookiePolicy = true;

            Response.SetCookie(cookie);

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return RedirectToAction("index");

            searchTerm = searchTerm.Replace(",", "").Trim();

            var isNumber = int.TryParse(searchTerm, out var height);

            if (isNumber)
                return RedirectToRoute("blocks", new {blockId = height});

            switch (searchTerm.Length)
            {
                case 51:
                    return RedirectToRoute("addresses", new { addressHash = searchTerm });
                case 128:
                    return RedirectToRoute("transactions", new { txHash = searchTerm });
                case 256:
                    return RedirectToRoute("blocks", new { blockId = searchTerm });
                default:
                    if (searchTerm.Length >= 20)
                        return RedirectToRoute("blocks", new { blockId = searchTerm });

                    return RedirectToAction("index");
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetLatestTimeStampUtc()
        {
            return Json(await _redisCommand.GetAsync<DateTime>(Settings.Redis.TimestampUtcLatest));;
        }
    }
}
