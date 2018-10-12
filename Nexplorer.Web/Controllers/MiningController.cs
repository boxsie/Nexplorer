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
    public class MiningController : WebControllerBase
    {
        private readonly RedisCommand _redisCommand;

        private const int ChartDurationMs = 300000;

        public MiningController(RedisCommand redisCommand)
        {
            _redisCommand = redisCommand;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new MiningViewModel
            {
                ChartDurationMs = ChartDurationMs,
                ChannelStats = await CreateRecentChannelStatsAsync(),
                SuppyRates = await _redisCommand.GetAsync<SupplyRateDto>(Settings.Redis.SupplyRatesLatest)
            };

            return View(vm);
        }

        private async Task<Dictionary<string, List<ChannelStatDto>>> CreateRecentChannelStatsAsync()
        {
            var recentMiningInfos = await _redisCommand.GetAsync<List<MiningInfoDto>>(Settings.Redis.MiningInfo10Mins);

            var miningInfos = recentMiningInfos
                .Where(x => x.CreatedOn > DateTime.UtcNow.AddMilliseconds(-ChartDurationMs))
                .ToList();


            var hashStats = new List<ChannelStatDto>();
            var primeStats = new List<ChannelStatDto>();
            var posStats = new List<ChannelStatDto>();

            foreach (var miningInfo in miningInfos)
            {
                hashStats.Add(new ChannelStatDto
                {
                    Channel = BlockChannels.Hash.ToString(),
                    CreatedOn = miningInfo.CreatedOn,
                    Difficulty = miningInfo.HashDifficulty,
                    RatePerSecond = miningInfo.HashPerSecond,
                    Reserve = miningInfo.HashReserve,
                    Reward = miningInfo.HashValue
                });

                primeStats.Add(new ChannelStatDto
                {
                    Channel = BlockChannels.Prime.ToString(),
                    CreatedOn = miningInfo.CreatedOn,
                    Difficulty = miningInfo.PrimeDifficulty,
                    RatePerSecond = miningInfo.PrimesPerSecond,
                    Reserve = miningInfo.PrimeReserve,
                    Reward = miningInfo.PrimeValue
                });

                posStats.Add(new ChannelStatDto
                {
                    Channel = BlockChannels.PoS.ToString(),
                    CreatedOn = miningInfo.CreatedOn,
                });
            }

            return new Dictionary<string, List<ChannelStatDto>>
            {
                {BlockChannels.Hash.ToString().ToLower(), hashStats},
                {BlockChannels.Prime.ToString().ToLower(), primeStats},
                {BlockChannels.PoS.ToString().ToLower(), posStats}
            };
        }
    }
}