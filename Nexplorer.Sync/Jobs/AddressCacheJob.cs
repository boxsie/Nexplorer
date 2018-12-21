using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Cache;
using Nexplorer.Data.Context;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Sync.Core;
using Quartz;

namespace Nexplorer.Sync.Jobs
{
    public class AddressCacheJob : SyncJob
    {
        private readonly NexusQuery _nexusQuery;
        private readonly NexusDb _nexusDb;
        private readonly BlockQuery _blockQuery;
        private readonly AddressQuery _addressQuery;
        private readonly RedisCommand _redisCommand;

        public AddressCacheJob(ILogger<AddressCacheJob> logger, NexusQuery nexusQuery, NexusDb nexusDb, BlockQuery blockQuery,
            AddressQuery addressQuery, RedisCommand redisCommand) : base(logger, 60)
        {
            _nexusQuery = nexusQuery;
            _nexusDb = nexusDb;
            _blockQuery = blockQuery;
            _addressQuery = addressQuery;
            _redisCommand = redisCommand;
        }
        
        protected override async Task<string> ExecuteAsync()
        {
            var trustResponse = await CacheTrustKeys();
            var nexusResponse = await CacheNexusAddresses();

            return $"{trustResponse} {nexusResponse}";
        }

        private async Task<string> CacheTrustKeys()
        {
            var trustKeys = await _nexusQuery.GetTrustKeys();

            var keyCache = await _redisCommand.GetAsync<List<TrustKeyDto>>(Settings.Redis.TrustKeyCache)
                           ?? new List<TrustKeyDto>();

            var keyAddressCache = await _redisCommand.GetAsync<List<AddressLiteDto>>(Settings.Redis.TrustKeyAddressCache)
                                  ?? new List<AddressLiteDto>();

            var expiredKeys = keyCache
                .Where(x => trustKeys.All(y => y.TrustKey != x.TrustKey))
                .ToList();

            await RemoveExpiredKeysAsync(expiredKeys, keyCache);
            await AddOrUpdateTrustKeysAsync(trustKeys, keyCache);

            var newKeyAddressCache = await AddOrUpdateTrustKeyAddressCache(keyCache, keyAddressCache);

            await _redisCommand.SetAsync(Settings.Redis.TrustKeyCache, keyCache);
            await _redisCommand.SetAsync(Settings.Redis.TrustKeyAddressCache, newKeyAddressCache);

            return $"Trust keys updated {trustKeys.Count} and expired {expiredKeys.Count}";
        }

        private async Task RemoveExpiredKeysAsync(List<TrustKeyDto> expiredKeys, List<TrustKeyDto> keyCache)
        {
            foreach (var expiredKey in expiredKeys)
            {
                await _nexusDb.AddAsync(new TrustKey
                {
                    GenesisBlock = await _nexusDb.Blocks.FirstOrDefaultAsync(x => x.Height == expiredKey.GenesisBlockHeight),
                    Address = await _nexusDb.Addresses.FirstOrDefaultAsync(x => x.AddressId == expiredKey.AddressId),
                    Transaction = await _nexusDb.Transactions.FirstOrDefaultAsync(x => x.TransactionId == expiredKey.TransactionId),
                    Hash = expiredKey.TrustHash,
                    Key = expiredKey.TrustKey,
                    CreatedOn = DateTime.Now
                });

                keyCache.RemoveAll(x => x.TrustKey == expiredKey.TrustKey);
            }

            await _nexusDb.SaveChangesAsync();
        }

        private async Task AddOrUpdateTrustKeysAsync(IEnumerable<TrustKeyResponseDto> latestKeys, ICollection<TrustKeyDto> keyCache)
        {
            var nonExpired = latestKeys.Where(x => !x.Expired).ToList();

            foreach (var latestKeyDto in nonExpired)
            {
                var trustKey = keyCache.FirstOrDefault(x => x.TrustKey == latestKeyDto.TrustKey);

                if (trustKey == null)
                {
                    var address = await _addressQuery.GetAddressAsync(latestKeyDto.AddressHash);
                    var block = await _blockQuery.GetBlockAsync(latestKeyDto.GenesisBlockHash);
                    var tx = block?.Transactions.FirstOrDefault(x => x.Hash == latestKeyDto.TransactionHash);

                    if (address == null || block == null || tx == null)
                        continue;

                    trustKey = new TrustKeyDto
                    {
                        AddressId = address.AddressId,
                        AddressHash = latestKeyDto.AddressHash,
                        GenesisBlockHeight = block.Height,
                        TransactionId = tx.TransactionId,
                        TransactionHash = tx.Hash,
                        InterestRate = latestKeyDto.InterestRate,
                        TimeSinceLastBlock = TimeSpan.FromSeconds(latestKeyDto.TimeSinceLastBlock),
                        TimeUtc = latestKeyDto.TimeUtc,
                        TrustHash = latestKeyDto.TrustHash,
                        TrustKey = latestKeyDto.TrustKey
                    };

                    keyCache.Add(trustKey);
                }
                else
                {
                    trustKey.InterestRate = latestKeyDto.InterestRate;
                    trustKey.TimeSinceLastBlock = TimeSpan.FromSeconds(latestKeyDto.TimeSinceLastBlock);
                }
            }
        }

        private async Task<List<AddressLiteDto>> AddOrUpdateTrustKeyAddressCache(IEnumerable<TrustKeyDto> keyCache, ICollection<AddressLiteDto> keyAddressCache)
        {
            var latestAddresses = new List<AddressLiteDto>();

            foreach (var keyVal in keyCache)
            {
                var keyAdd = keyAddressCache.FirstOrDefault(x => x.Hash == keyVal.AddressHash) 
                             ?? await _addressQuery.GetAddressLiteAsync(keyVal.AddressHash);

                keyAdd.InterestRate = keyVal.InterestRate;

                latestAddresses.Add(keyAdd);
            }

            return latestAddresses;
        }
        
        private async Task<string> CacheNexusAddresses()
        {
            var nexusAddresses = new List<AddressLiteDto>();

            var hashes = AddressQuery._nexusAddresses.Values.SelectMany(x => x.SelectMany(y => y));

            foreach (var nexusAmbassadorAddress in hashes)
            {
                var address = await _addressQuery.GetAddressLiteAsync(nexusAmbassadorAddress);

                address.IsNexus = true;

                nexusAddresses.Add(address);
            }

            await _redisCommand.SetAsync(Settings.Redis.NexusAddressCache, nexusAddresses);

            return $"{nexusAddresses.Count} Nexus addresses updated";
        }
    }
}