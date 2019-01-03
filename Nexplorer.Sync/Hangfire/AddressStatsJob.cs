using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Criteria;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Sync.Hangfire
{
    public class AddressStatsJob
    {
        public static readonly TimeSpan JobInterval = TimeSpan.FromMinutes(5);

        private const int TimeoutSeconds = 10;
        private const int StakeThreshold = 1000;

        private readonly ILogger<AddressStatsJob> _logger;
        private readonly RedisCommand _redisCommand;
        private readonly AddressQuery _addressQuery;
        private readonly BlockQuery _blockQuery;
        private readonly NexusQuery _nexusQuery;

        public AddressStatsJob(ILogger<AddressStatsJob> logger, RedisCommand redisCommand, AddressQuery addressQuery, BlockQuery blockQuery, NexusQuery nexusQuery)
        {
            _logger = logger;
            _redisCommand = redisCommand;
            _addressQuery = addressQuery;
            _blockQuery = blockQuery;
            _nexusQuery = nexusQuery;
        }

        [DisableConcurrentExecution(TimeoutSeconds)]
        public async Task UpdateStatsAsync()
        {
            var sw = new Stopwatch();
            
            sw.Start();

            _logger.LogInformation("Updating address stats...");

            var dormantThreshold = await _blockQuery.GetBlockAsync(DateTime.Now.AddYears(-1));
            
            var addressStats = await _redisCommand.GetAsync<AddressStatDto>(Settings.Redis.AddressStatPubSub) ?? new AddressStatDto();

            addressStats.AddressCount = await _addressQuery.GetUniqueAddressCountAsync();
            addressStats.CreatedPerHour = await _addressQuery.GetAddressesCreatedLastHourAsync();
            addressStats.ZeroBalance = (int)(await _addressQuery.GetCountFilteredAsync(new AddressFilterCriteria { MaxBalance = 0 }));

            var stakeableAddresses = await _addressQuery.GetAddressLitesFilteredAsync(new AddressFilterCriteria
            {
                OrderBy = OrderAddressesBy.HighestBalance,
                MinBalance = StakeThreshold
            }, 0, int.MaxValue, false);
            addressStats.BalanceOverOneThousand = stakeableAddresses.Results.Count;

            var oldAddresses = await _addressQuery.GetAddressLitesFilteredAsync(new AddressFilterCriteria
            {
                OrderBy = OrderAddressesBy.MostRecentlyActive,
                MinBalance = 0.00000001d,
                HeightTo = dormantThreshold.Height
            }, 0, int.MaxValue, false);
            addressStats.DormantOverOneYear = oldAddresses.Results.Count;

            var stakingAddresses = await _addressQuery.GetAddressLitesFilteredAsync(new AddressFilterCriteria
            {
                IsStaking = true
            }, 0, int.MaxValue, false);
            addressStats.StakingCount = stakingAddresses.Results.Count;
            addressStats.TotalStakedCoins = Math.Round(stakingAddresses.Results.Sum(x => x.Balance), 8);

            addressStats.AverageBalance = await _addressQuery.GetAverageBalanceAsync(false);

            var distributionBands = Enum.GetValues(typeof(AddressBalanceDistributionBands))
                .Cast<AddressBalanceDistributionBands>()
                .ToList();

            var distributionBalances = new List<double>();
            var distributionCounts = new List<int>();
            var supplyInfo = await _nexusQuery.GetSupplyRate();

            var distributionDtos = new List<AddressDistrubtionBandDto>();

            foreach (var distributionBand in distributionBands)
            {
                var bandFilter = GetDistrubutionBands(distributionBand);

                var addCount = (int)(await _addressQuery.GetCountFilteredAsync(bandFilter));
                var coinBalance = await _addressQuery.GetBalanceSumFilteredAsync(bandFilter);

                distributionDtos.Add(new AddressDistrubtionBandDto
                {
                    DistributionBand = distributionBand,
                    AddressCount = addCount,
                    AddressPercent = (addCount / (double)(addressStats.AddressCount - addressStats.ZeroBalance)) * 100,
                    CoinBalance = coinBalance,
                    CoinPercent = (coinBalance / supplyInfo.MoneySupply) * 100
                });
            }

            await _redisCommand.SetAsync(Settings.Redis.AddressDistributionStats, distributionDtos);

            await _redisCommand.SetAsync(Settings.Redis.AddressStatPubSub, addressStats);
            await _redisCommand.PublishAsync(Settings.Redis.AddressStatPubSub, addressStats);

            _logger.LogInformation($"Updated address stats in {sw.Elapsed:g}");

            BackgroundJob.Schedule<AddressStatsJob>(x => x.UpdateStatsAsync(), JobInterval);
        }

        private AddressFilterCriteria GetDistrubutionBands(AddressBalanceDistributionBands band)
        {
            switch (band)
            {
                case AddressBalanceDistributionBands.OverZeroToTen:
                    return new AddressFilterCriteria { MinBalance = 0.00000001d, MaxBalance = 10 };
                case AddressBalanceDistributionBands.OverTenToOneHundred:
                    return new AddressFilterCriteria { MinBalance = 10.00000001d, MaxBalance = 100 };
                case AddressBalanceDistributionBands.OverOneHundredToOneThousand:
                    return new AddressFilterCriteria { MinBalance = 100.00000001d, MaxBalance = 1000 };
                case AddressBalanceDistributionBands.OverOneThousandToTenThousand:
                    return new AddressFilterCriteria { MinBalance = 1000.00000001d, MaxBalance = 10000 };
                case AddressBalanceDistributionBands.OverTenThousandToOneHundredThousand:
                    return new AddressFilterCriteria { MinBalance = 10000.00000001d, MaxBalance = 100000 };
                case AddressBalanceDistributionBands.OverOneHundredThousandToOneMillion:
                    return new AddressFilterCriteria { MinBalance = 100000.00000001d, MaxBalance = 1000000 };
                case AddressBalanceDistributionBands.OneMillionPlus:
                    return new AddressFilterCriteria { MinBalance = 1000000.00000001d };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}