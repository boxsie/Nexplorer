using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;
using Nexplorer.Sync.Core;

namespace Nexplorer.Sync.Jobs
{
    public class AddressStats : SyncJob
    {
        private const int IntervalSeconds = 30;
        private const int BigUpdateIntervalSeconds = 60 * 60;
        private static int _secondsSinceLastBigUpdate = 0;

        private readonly RedisCommand _redisCommand;
        private readonly AddressQuery _addressQuery;
        private readonly BlockQuery _blockQuery;
        private readonly NexusQuery _nexusQuery;

        public AddressStats(ILogger<AddressStats> logger, RedisCommand redisCommand, AddressQuery addressQuery, BlockQuery blockQuery, NexusQuery nexusQuery)
            : base(logger, IntervalSeconds)
        {
            _redisCommand = redisCommand;
            _addressQuery = addressQuery;
            _blockQuery = blockQuery;
            _nexusQuery = nexusQuery;
        }

        protected override async Task<string> ExecuteAsync()
        {
            const int stakeThreshold = 1000;
            var dormantThreshold = await _blockQuery.GetBlockAsync(DateTime.Now.AddYears(-1));

            _secondsSinceLastBigUpdate += IntervalSeconds;
            
            var addressStats = await _redisCommand.GetAsync<AddressStatDto>(Settings.Redis.AddressStatPubSub);
            var noStats = false;

            if (addressStats == null)
            {
                addressStats = new AddressStatDto();
                noStats = true;
            }

            addressStats.AddressCount = await _addressQuery.GetUniqueAddressCountAsync();
            addressStats.CreatedPerHour = await _addressQuery.GetAddressesCreatedLastHourAsync();
            addressStats.StakingCount = await _addressQuery.GetTrustKeyCountAsync();
            addressStats.ZeroBalance = (int)(await _addressQuery.GetCountFilteredAsync(new AddressFilterCriteria { MaxBalance = 0 }));

            if (noStats || _secondsSinceLastBigUpdate > BigUpdateIntervalSeconds)
            {
                _secondsSinceLastBigUpdate = 0;

                var stakeableAddresses = await _addressQuery.GetAddressLitesFilteredAsync(new AddressFilterCriteria
                {
                    OrderBy = OrderAddressesBy.HighestBalance,
                    MinBalance = stakeThreshold
                }, 0, int.MaxValue, false);

                var oldAddresses = await _addressQuery.GetAddressLitesFilteredAsync(new AddressFilterCriteria
                {
                    OrderBy = OrderAddressesBy.MostRecentlyActive,
                    MinBalance = 0.00000001d,
                    HeightTo = dormantThreshold.Height
                }, 0, int.MaxValue, false);

                addressStats.AverageBalance = await _addressQuery.GetAverageBalanceAsync();
                addressStats.BalanceOverOneThousand = stakeableAddresses.Addresses.Count;
                addressStats.DormantOverOneYear = oldAddresses.Addresses.Count;

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
            }
            
            await _redisCommand.SetAsync(Settings.Redis.AddressStatPubSub, addressStats);
            await _redisCommand.PublishAsync(Settings.Redis.AddressStatPubSub, addressStats);
            
            return "Updated address stats";
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