using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;
using Nexplorer.Web.Models;

namespace Nexplorer.Web.Controllers
{
    public class StakingController : WebControllerBase
    {
        private readonly RedisCommand _redisCommand;
       
        public StakingController(RedisCommand redisCommand)
        {
            _redisCommand = redisCommand;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new StakingViewModel()
            {

            };

            return View(vm);
        }
    }
}