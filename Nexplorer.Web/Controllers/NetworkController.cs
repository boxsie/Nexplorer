using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Domain.Dtos;
using Nexplorer.Web.Models;

namespace Nexplorer.Web.Controllers
{
    public class NetworkController : WebControllerBase
    {
        private readonly RedisCommand _redisCommand;

        public NetworkController(RedisCommand redisCommand)
        {
            _redisCommand = redisCommand;
        }

        public async Task<IActionResult> Index()
        {
            var viemModel = new NetworkViewModel
            {
                PeerInfoDtos = await _redisCommand.GetAsync<List<PeerInfoDto>>(Settings.Redis.PeerInfoLatest)
            };

            return View(viemModel);
        }
    }
}