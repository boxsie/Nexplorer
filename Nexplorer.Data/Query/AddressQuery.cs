using System;
using Dapper;
using Nexplorer.Data.Context;
using Nexplorer.Domain.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Data.Query
{
    public class AddressQuery
    {
        private readonly NexusDb _nexusDb;
        private readonly RedisCommand _redisCommand;
        private readonly BlockCacheService _blockCache;

        public AddressQuery(NexusDb nexusDb, RedisCommand redisCommand, BlockCacheService blockCache)
        {
            _nexusDb = nexusDb;
            _redisCommand = redisCommand;
            _blockCache = blockCache;
        }

        public async Task<AddressDto> GetAddressAsync(string addressHash)
        {
            var addressId = await GetAddressIdAsync(addressHash);

            return await GetAddressAsync(addressId, addressHash);
        }

        public async Task<AddressDto> GetAddressAsync(int addressId)
        {
            var addressHash = await GetAddressHashAsync(addressId);

            return await GetAddressAsync(addressId, addressHash);
        }

        public async Task<AddressLiteDto> GetAddressLiteAsync(string addressHash)
        {
            var addressId = await GetAddressIdAsync(addressHash);

            return await GetAddressLiteAsync(addressId, addressHash);
        }

        public async Task<AddressLiteDto> GetAddressLiteAsync(int addressId)
        {
            var addressHash = await GetAddressHashAsync(addressId);

            return await GetAddressLiteAsync(addressId, addressHash);
        }

        public async Task<List<AddressTransactionDto>> GetAddressTransactionsAsync(int addressId, TransactionType txType, int? start = null, int? count = null)
        {
            var addressHash = await GetAddressHashAsync(addressId);

            return await GetAddressTransactionsInOutAsync(txType, addressId, addressHash, start, count);
        }

        public async Task<List<AddressTransactionDto>> GetAddressTransactionsAsync(string addressHash, TransactionType txType, int? start = null, int? count = null)
        {
            var addressId = await GetAddressIdAsync(addressHash);

            return await GetAddressTransactionsInOutAsync(txType, addressId, addressHash, start, count);
        }

        public async Task<TrustKeyDto> GetAddressTrustKey(string addressHash)
        {
            var truskKeyCache = await _redisCommand.GetAsync<List<TrustKeyDto>>(Settings.Redis.TrustKeyCache);

            return truskKeyCache.FirstOrDefault(x => x.AddressHash == addressHash);
        }

        public async Task<int> GetTrustKeyCountAsync()
        {
            var truskKeyCache = await _redisCommand.GetAsync<List<TrustKeyDto>>(Settings.Redis.TrustKeyCache);

            return truskKeyCache?.Count ?? 0;
        }

        public int GetNexusAddressCount()
        {
            return NexusAmbassadorAddresses.Length + NexusDevAddresses.Length + NexusEmbassyAddresses.Length;
        }

        public async Task<string> GetAddressHashAsync(int addressId)
        {
            return await _nexusDb.Addresses
                .Where(x => x.AddressId == addressId)
                .Select(x => x.Hash)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetAddressIdAsync(string addressHash)
        {
            return await _nexusDb.Addresses
                .Where(x => x.Hash == addressHash)
                .Select(x => x.AddressId)
                .FirstOrDefaultAsync();
        }

        public async Task<AddressDto> GetAddressAsync(int addressId, string addressHash)
        {
            const string sqlQ = @"SELECT
                                  a.AddressId,
                                  a.Hash,
                                  a.FirstBlockHeight AS FirstBlockSeen,
                                  aa.LastBlockHeight  AS LastBlockSeen,
                                  aa.ReceivedAmount,
                                  aa.ReceivedCount,
                                  aa.SentAmount,
                                  aa.SentCount
                                  FROM Address a
                                  LEFT JOIN AddressAggregate aa ON aa.AddressId = a.AddressId
                                  WHERE a.AddressId = @addressId;";

            using (var sqlCon = await DbConnectionFactory.GetNexusDbConnectionAsync())
            {
                var dbAddress = (await sqlCon.QueryAsync<AddressDto>(sqlQ, new {addressId})).FirstOrDefault();

                return addressHash == null 
                    ? dbAddress 
                    : MapCacheAddressToAddress(await _blockCache.GetAddressAsync(addressHash), dbAddress);
            }
        }

        public async Task<AddressLiteDto> GetAddressLiteAsync(int addressId, string addressHash)
        {
            const string sqlQ = @"SELECT
                                  a.AddressId,
                                  a.Hash,
                                  a.FirstBlockHeight AS FirstBlockSeen,
                                  aa.LastBlockHeight  AS LastBlockSeen,
                                  aa.Balance
                                  FROM Address a
                                  LEFT JOIN AddressAggregate aa ON aa.AddressId = a.AddressId
                                  WHERE a.AddressId = @addressId;";

            using (var sqlCon = await DbConnectionFactory.GetNexusDbConnectionAsync())
            {
                var dbAddress = (await sqlCon.QueryAsync<AddressLiteDto>(sqlQ, new { addressId })).FirstOrDefault();

                return addressHash == null 
                    ? dbAddress 
                    : MapCacheAddressToAddressLite(await _blockCache.GetAddressAsync(addressHash), dbAddress);
            }
        }

        public async Task<double> GetBalanceSumFilteredAsync(AddressFilterCriteria filter)
        {
            var min = filter.MinBalance ?? 0;
            var max = filter.MaxBalance ?? double.MaxValue;
            var fromHeight = filter.HeightFrom ?? 0;
            var toHeight = filter.HeightTo ?? int.MaxValue;

            var sqlQ = $@"SELECT
                          SUM(aa.Balance)
                          FROM AddressAggregate aa
                          WHERE aa.Balance >= @min AND aa.Balance <= @max AND aa.LastBlockHeight >= @fromHeight AND aa.LastBlockHeight <= @toHeight;";

            using (var sqlCon = await DbConnectionFactory.GetNexusDbConnectionAsync())
            {
                var param = new { min, max, fromHeight, toHeight };

                return (await sqlCon.QueryAsync<double>(sqlQ, param)).FirstOrDefault();
            }
        }

        public async Task<long> GetCountFilteredAsync(AddressFilterCriteria filter)
        {
            var min = filter.MinBalance ?? 0;
            var max = filter.MaxBalance ?? double.MaxValue;
            var fromHeight = filter.HeightFrom ?? 0;
            var toHeight = filter.HeightTo ?? int.MaxValue;

            var sqlQ = $@"SELECT
                          COUNT(1)
                          FROM AddressAggregate aa
                          WHERE aa.Balance >= @min AND aa.Balance <= @max AND aa.LastBlockHeight >= @fromHeight AND aa.LastBlockHeight <= @toHeight;";

            using (var sqlCon = await DbConnectionFactory.GetNexusDbConnectionAsync())
            {
                var param = new { min, max, fromHeight, toHeight };

                return (await sqlCon.QueryAsync<long>(sqlQ, param)).FirstOrDefault();
            }
        }

        public async Task<AddressFilterResult> GetAddressLitesFilteredAsync(AddressFilterCriteria filter, int start, int count, bool countResults, int? maxResults = null)
        {
            var min = filter.MinBalance ?? 0;
            var max = filter.MaxBalance ?? double.MaxValue;
            var fromHeight = filter.HeightFrom ?? 0;
            var toHeight = filter.HeightTo ?? int.MaxValue;

            var isNxs = filter.IsNexus;
            var isStk = filter.IsStaking;

            var trustAddresses = await _redisCommand.GetAsync<List<AddressLiteDto>>(Settings.Redis.TrustKeyAddressCache);
            var nexusAddresses = await _redisCommand.GetAsync<List<AddressLiteDto>>(Settings.Redis.NexusAddressCache);

            if (isNxs || isStk)
            {
                var adds = (!isNxs || !isStk) 
                    ? isNxs 
                        ? nexusAddresses 
                        : trustAddresses 
                    : trustAddresses.Where(x => nexusAddresses.Any(y => y.Hash == x.Hash));

                var filtered = adds.Where(x => x.Balance >= min && x.Balance <= max && x.LastBlockSeen >= fromHeight && x.LastBlockSeen <= toHeight);

                var ordered = OrderAddresses(filtered, filter.OrderBy).ToList();

                var result = new AddressFilterResult
                {
                    ResultCount = ordered.Count,
                    Addresses = ordered.Skip(start).Take(count).ToList()
                };

                return result;
            }

            var sqlOrderBy = "ORDER BY";

            switch (filter.OrderBy)
            {
                case OrderAddressesBy.LowestBalance:
                    sqlOrderBy += " aa.Balance";
                    break;
                case OrderAddressesBy.MostRecentlyActive:
                    sqlOrderBy += " LastBlockSeen DESC";
                    break;
                case OrderAddressesBy.LeastRecentlyActive:
                    sqlOrderBy += " LastBlockSeen";
                    break;
                default:
                    sqlOrderBy += " aa.Balance DESC";
                    break;
            }

            var sqlQ = $@"SELECT
                          a.AddressId,
                          a.Hash,
                          a.FirstBlockHeight AS FirstBlockSeen,
                          aa.LastBlockHeight AS LastBlockSeen,
                          aa.Balance
                          FROM Address a
                          LEFT JOIN AddressAggregate aa ON aa.AddressId = a.AddressId
                          WHERE aa.Balance >= @min AND aa.Balance <= @max AND aa.LastBlockHeight >= @fromHeight AND aa.LastBlockHeight <= @toHeight 
                          {sqlOrderBy}
                          OFFSET @start ROWS FETCH NEXT @count ROWS ONLY;";

            var sqlC = @"SELECT 
                         COUNT(*)
                         FROM (SELECT TOP @maxResults
                               1
                               FROM Address a 
                               LEFT JOIN AddressAggregate aa ON aa.AddressId = a.AddressId 
                               WHERE aa.Balance >= @min AND aa.Balance <= @max AND aa.LastBlockHeight >= @fromHeight AND aa.LastBlockHeight <= @toHeight) AS resultCount;";

            using (var sqlCon = await DbConnectionFactory.GetNexusDbConnectionAsync())
            {
                var results = new AddressFilterResult();
                var param = new {min, max, fromHeight, toHeight, start, count, maxResults = maxResults ?? int.MaxValue};

                if (countResults)
                {
                    using (var multi = await sqlCon.QueryMultipleAsync(string.Concat(sqlQ, sqlC), param))
                    {
                        results.Addresses = (await multi.ReadAsync<AddressLiteDto>()).ToList();
                        results.ResultCount = (int)(await multi.ReadAsync<long>()).FirstOrDefault();
                    }
                }
                else
                {
                    results.Addresses = (await sqlCon.QueryAsync<AddressLiteDto>(sqlQ, param)).ToList();
                    results.ResultCount = -1;
                }

                foreach (var address in results.Addresses)
                {
                    address.InterestRate = trustAddresses?.FirstOrDefault(x => x.Hash == address.Hash)?.InterestRate;
                    address.IsNexus = nexusAddresses?.Any(x => x.Hash == address.Hash) ?? false;
                }

                return results;
            }
        }

        public async Task<List<AddressTransactionDto>> GetAddressTransactionsInOutAsync(TransactionType txType, int addressId, string addressHash, int? start = null, int? count = null)
        {
            var sqlQ = "";

            switch (txType)
            {
                case TransactionType.Input:
                    sqlQ += @"SELECT
                              1 AS TxType,
                              t.BlockHeight,
                              t.Hash AS TransactionHash,
                              tIn.Amount,
                              t.Timestamp
                              FROM Transaction t
                              INNER JOIN TransactionInput tIn ON tIn.TransactionId = t.TransactionId
                              INNER JOIN Address a ON a.AddressId = tIn.AddressId
                              WHERE a.AddressId = @addressId
                              ORDER BY t.Timestamp DESC";
                    break;
                case TransactionType.Output:
                    sqlQ += @"SELECT
                              2 AS TxType,
                              t.BlockHeight,
                              t.Hash AS TransactionHash,
                              tOut.Amount,
                              t.Timestamp
                              FROM Transaction t
                              INNER JOIN TransactionOutput tOut ON tOut.TransactionId = t.TransactionId
                              INNER JOIN Address a ON a.AddressId = tOut.AddressId
                              WHERE a.AddressId = @addressId
                              ORDER BY t.Timestamp DESC";
                    break;
                case TransactionType.Both:
                    sqlQ += @"SELECT 
                              2 AS TxType,
                              t.Hash AS TransactionHash,
                              t.BlockHeight,
                              tOut.Amount,
                              t.Timestamp
                              FROM TransactionOutput tOut
                              INNER JOIN Transaction t On t.TransactionId = tOut.TransactionId
                              WHERE tOut.AddressId = @addressId
                              UNION ALL                        
                              SELECT
                              1 AS TxType,
                              t.Hash AS TransactionHash,
                              t.BlockHeight,
                              tIn.Amount,
                              t.Timestamp
                              FROM TransactionInput tIn
                              INNER JOIN Transaction t On t.TransactionId = tIn.TransactionId
                              WHERE tIn.AddressId = @addressId                         
                              ORDER BY Timestamp DESC, TxType DESC";
                    break;
            }

            if (start.HasValue && count.HasValue)
                sqlQ += " OFFSET @start ROWS FETCH NEXT @count ROWS ONLY; ";

            using (var sqlCon = await DbConnectionFactory.GetNexusDbConnectionAsync())
            {
                var cacheTxs = (await _blockCache.GetAddressTransactions(addressHash))
                    .Where(x => txType == TransactionType.Both || x.TxType == txType).ToList();
                    
                var cacheTxCount = cacheTxs.Count;
                    
                cacheTxs = cacheTxs
                    .Skip(start ?? 0)
                    .Take(count ?? 5)
                    .ToList();

                if (cacheTxs.Count >= count)
                    return cacheTxs;

                var dbStart = (start ?? 0) - cacheTxCount;
                var dbCount = (count ?? 5) - cacheTxs.Count;

                if (dbStart < 0)
                    dbStart = 0;

                var dbTxs = (await sqlCon.QueryAsync(sqlQ, new { addressId, start = dbStart, count = dbCount }))
                    .Select(x => new AddressTransactionDto
                    {
                        TxType = (TransactionType)x.TxType,
                        BlockHeight = x.BlockHeight,
                        TransactionHash = x.TransactionHash,
                        Amount = x.Amount,
                        TimeUtc = x.TimeUtc
                    }).ToList();

                return dbTxs
                    .Concat(cacheTxs)
                    .OrderByDescending(x => x.TimeUtc)
                    .ThenByDescending(x => (int)x.TxType)
                    .ToList();
            }
        }

        public async Task<List<AddressBalanceDto>> GetAddressBalance(string addressHash, int days)
        {
            var addressId = await GetAddressIdAsync(addressHash);

            const string sqlQ = @"SELECT 
                                  2 AS TxType,
                                  tOut.Amount,
                                  t.Timestamp
                                  FROM TransactionOutput tOut
                                  INNER JOIN Transaction t On t.TransactionId = tOut.TransactionId
                                  WHERE tOut.AddressId = @addressId
                                  AND t.Timestamp >= @fromDate                         
                                  UNION ALL
                                  SELECT
                                  1 AS TxType,
                                  tIn.Amount,
                                  t.Timestamp
                                  FROM TransactionInput tIn
                                  INNER JOIN Transaction t On t.TransactionId = tIn.TransactionId
                                  WHERE tIn.AddressId = @addressId
                                  AND t.Timestamp >= @fromDate                         
                                  ORDER BY Timestamp, TxType DESC";

            using (var sqlCon = await DbConnectionFactory.GetNexusDbConnectionAsync())
            {
                // The amount negative is the reversed because we are 
                // working backwards and substracting from the current balance

                var dbBalances = (await sqlCon.QueryAsync(sqlQ, new { addressId, fromDate = DateTime.Now.AddDays(-days) }))
                    .Select(x => new
                    {
                        ((DateTime)x.TimeUtc).Date,
                        Balance = (double)((int)x.TxType == 1 ? x.Amount : -x.Amount)
                    }).ToList();
                
                var cacheBalances = (await _blockCache.GetAddressTransactions(addressHash))
                    .Select(x => new
                    {
                        x.TimeUtc.Date,
                        Balance = (int)x.TxType == 1 ? x.Amount : -x.Amount
                    }).ToList();
                
                var allBalances = dbBalances
                    .Concat(cacheBalances)
                    .GroupBy(x => x.Date)
                    .Select(x => new AddressBalanceDto
                    {
                        Date = x.Key,
                        Balance = x.Sum(y => y.Balance)
                    }).ToList();

                var currentBalance = await _nexusDb
                    .AddressAggregates
                    .Where(x => x.AddressId == addressId)
                    .Select(x => x.Balance)
                    .FirstOrDefaultAsync();

                currentBalance = Math.Round(currentBalance + cacheBalances.Sum(x => -x.Balance), 8);

                var finalBalances = new List<AddressBalanceDto>
                {
                    new AddressBalanceDto
                    {
                        Date = DateTime.Now.Date,
                        Balance = Math.Abs(currentBalance)
                    }
                };

                for (var i = 1; i < days; i++)
                {
                    var date = DateTime.Now.AddDays(-i).Date;
                    var balance = allBalances.FirstOrDefault(x => x.Date == finalBalances.Last().Date);

                    if (balance != null)
                        currentBalance = Math.Round(currentBalance + balance.Balance, 8);

                    finalBalances.Add(new AddressBalanceDto
                    {
                        Date = date,
                        Balance = currentBalance
                    });
                }

                return finalBalances
                    .OrderBy(x => x.Date)
                    .ToList();
            }
        }

        public async Task<long> GetUniqueAddressCountAsync()
        {
            return await _nexusDb.Addresses.CountAsync();
        }

        public async Task<double> GetAverageBalanceAsync()
        {
            const string sqlQ = @"SELECT
                                  AVG(aa.Balance)
                                  FROM AddressAggregate aa;";

            using (var sqlCon = await DbConnectionFactory.GetNexusDbConnectionAsync())
            {
                var result = await sqlCon.QueryAsync<double>(sqlQ);

                return result.FirstOrDefault();
            }
        }

        public async Task<int> GetAddressesCreatedLastHourAsync()
        {
            var mostRecent = await _nexusDb.Blocks
                .OrderByDescending(x => x.Height)
                .Select(x => x.Timestamp)
                .FirstOrDefaultAsync();

            return await _nexusDb.Addresses
                .Where(x => x.FirstBlock.Timestamp >= mostRecent.AddHours(-1))
                .CountAsync();
        }

        public Task<List<AddressDistrubtionBandDto>> GetAddressesDistributionBandsAsync()
        {
            return _redisCommand.GetAsync<List<AddressDistrubtionBandDto>>(Settings.Redis.AddressDistributionStats);
        }

        private IEnumerable<AddressLiteDto> OrderAddresses(IEnumerable<AddressLiteDto> addresses, OrderAddressesBy orderBy)
        {
            switch (orderBy)
            {
                case OrderAddressesBy.HighestBalance:
                    return addresses.OrderByDescending(x => x.Balance);
                case OrderAddressesBy.LowestBalance:
                    return addresses.OrderBy(x => x.Balance);
                case OrderAddressesBy.MostRecentlyActive:
                    return addresses.OrderByDescending(x => x.LastBlockSeen);
                case OrderAddressesBy.LeastRecentlyActive:
                    return addresses.OrderBy(x => x.LastBlockSeen);
                case OrderAddressesBy.HighestInterestRate:
                    return addresses.OrderByDescending(x => x.InterestRate);
                case OrderAddressesBy.LowestInterestRate:
                    return addresses.OrderBy(x => x.InterestRate);
                default:
                    throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null);
            }
        }

        private static AddressDto MapCacheAddressToAddress(CachedAddressDto cacheAddress, AddressDto address)
        {
            if (cacheAddress == null)
                return address;

            if (address == null)
            {
                address = new AddressDto
                {
                    Hash = cacheAddress.Hash,
                    FirstBlockSeen = int.MaxValue,
                    LastBlockSeen = 0
                };
            }

            if (cacheAddress.FirstBlockHeight < address.FirstBlockSeen)
                address.FirstBlockSeen = cacheAddress.FirstBlockHeight;

            if (cacheAddress.Aggregate.LastBlockHeight > address.LastBlockSeen)
                address.LastBlockSeen = cacheAddress.Aggregate.LastBlockHeight;

            address.ReceivedAmount = Math.Round(address.ReceivedAmount + cacheAddress.Aggregate.ReceivedAmount, 8);
            address.SentAmount = Math.Round(address.SentAmount + cacheAddress.Aggregate.SentAmount, 8);

            address.ReceivedCount += cacheAddress.Aggregate.ReceivedCount;
            address.SentCount += cacheAddress.Aggregate.SentCount;

            return address;
        }

        private static AddressLiteDto MapCacheAddressToAddressLite(CachedAddressDto cacheAddress, AddressLiteDto address)
        {
            if (cacheAddress == null)
                return address;

            if (address == null)
            {
                address = new AddressLiteDto
                {
                    Hash = cacheAddress.Hash,
                    FirstBlockSeen = int.MaxValue,
                    LastBlockSeen = 0
                };
            }

            if (cacheAddress.FirstBlockHeight < address.FirstBlockSeen)
                address.FirstBlockSeen = cacheAddress.FirstBlockHeight;

            if (cacheAddress.Aggregate.LastBlockHeight > address.LastBlockSeen)
                address.LastBlockSeen = cacheAddress.Aggregate.LastBlockHeight;

            address.Balance = Math.Round(address.Balance + Math.Round(cacheAddress.Aggregate.ReceivedAmount - cacheAddress.Aggregate.SentAmount, 8), 8);

            return address;
        }

        public static readonly string[] NexusAmbassadorAddresses = new[]
        {
            "2Qn8MsUCkv9S8X8sMvUPkr9Ekqqg5VKzeKJk2ftcFMUiJ5QnWRc",
            "2S4WLATVCdJXTpcfdkDNDVKK5czDi4rjR5HrCRjayktJZ4rN8oA",
            "2RE29WahXWawQ9huhyaGhfvEMmUWHH9Hfo1anbNk8eW3nTU7H2g",
            "2QpSfg6MBZYCjQKXjTgo9eHoKMCJsYjLQsNT3xeeAYhrQmNBEUd",
            "2RHjigCh1qt1j3WKz4mShFBiVE5g6z9vrFpGMT6EDQsFJbtx4hr",
            "2SZ87FB1zukH5h7BLDT4yUyMTQnEJEt2KzpGYFxuUzMqAxEFN7Y",
            "2STyHuCeBgE81ZNjhH5QB8UXViXW7WPYM1YQgmXfLvMJXaKAFCs",
            "2SLq49uDrhLyP1N7Xnkj86WCHQUKxn6zx38LBNoTgwsAjfV1seq",
            "2RwtQdi3VPPQqht15QmXnS4KELgxrfaH2hXSywtJrfDdCJMnwPQ",
            "2SWscUR6rEezZKgFh5XkEyhmRmja2qrHMRsfmtxdapwMymmM96Q",
            "2SJzPMXNPEgW2zJW4489qeiCjdUanDTqCuSNAMmZXm1KX269jAt",
            "2Rk2mBEYWkGDMzQhEqdpSGZ77ZGvp9HWAbcsY6mDtbWKJy4DQuq",
            "2Rnh3qFvzeRQmSJEHtz6dNphq3r7uDSGQdjucnVFtpcuzBbeiLx"
        };

        public static readonly string[] NexusDevAddresses = new[]
        {
            "2Qp1rHzLCCsL4RcmLRcNhhAjnFShDXQaAjyBB9YpDSomCJsfGfS",
            "2SFx2tc8tLHBtkLkfK7nNjkU9DwvZZMNKKLaeX4xcG8ev4HQqVP",
            "2SawW67sUcVtLNarcAkVaFR2L1R8AWujkoryJHi8L47bdDP8hwC",
            "2QvzSNP3jy4MjqmB7jRy6jRGrDz6s6ALzTwu8htdohraU6Fdgrc",
            "2RxmzQ1XomQrbzQimajfnC2FubYnSzbwz5EkU2du7bDxuJW7i2L",
            "2S2JaEbGoBFx7N2YGEUJbWRjLf35tY7kezV8R9vzq9Wu1f5cwVz",
            "2S9bR5wB6RcBm1weFPBXCZai5pRFisa9zQSEedrdi9QLmd5Am8y",
            "2S6NjGDuTozyCWjMziwEBYKnnaC6fy5zRjDmd2NQhHBjuzKw4bg",
            "2RURDVPFD14eYCC7brgio2HF77LP22SdN5CeAvwQAwdSPdS95dT",
            "2SNAEJ6mbmpPXbP6ZkmH7FgtWTWcNnw2Pcs3Stb4PDaq3vH1GgE",
            "2SDnQuMgW9UdESUUekrrWegxSHLQWnFWJ2BNWAUQVecKNwBeNh5",
            "2SCLU4SKxh2P27foN9NRoAdtUZMdELMvBpfmVER98HayRRqGKFx",
            "2SLN8urU2mERZRQajqYe9VgQQgK7tPWWQ1679c5B3scZKP2vDxi"
        };

        public static readonly string[] NexusEmbassyAddresses = new[]
        {
            "2SCWx5hCFLsQMCyUJoDhhHDyzUa89tXBnjn3ytNsihpz2MqiC1D"
        };
    }
}