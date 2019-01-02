using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Nexplorer.Data.Cache.Services
{
    public class BlockCacheService
    {
        private readonly RedisCommand _redisCommand;
        private readonly ILogger<BlockCacheService> _logger;
        private List<BlockDto> _cache;

        public BlockCacheService(RedisCommand redisCommand, ILogger<BlockCacheService> logger)
        {
            _redisCommand = redisCommand;
            _logger = logger;


            _redisCommand.Subscribe<BlockLiteDto>(Settings.Redis.NewBlockPubSub, AddToCache);
        }

        public Task<BlockDto> GetBlockAsync(int height)
        {
            return _redisCommand.GetAsync<BlockDto>(Settings.Redis.BuildCachedBlockKey(height));
        }

        public async Task<BlockDto> GetBlockAsync(string hash)
        {
            var cache = await GetCacheAsync();

            var block = cache.FirstOrDefault(x => x.Hash == hash);

            return block ?? cache.FirstOrDefault(x => x.Hash.StartsWith(hash));
        }

        public async Task<string> GetBlockHashAsync(int height)
        {
            return (await GetBlockAsync(height))?.Hash;
        }

        public async Task<BlockDto> GetLastBlockAsync()
        {
            return (await GetBlockAsync(await GetCacheHeightAsync()));
        }

        public async Task<List<BlockDto>> GetBlocksAsync()
        {
            return await GetCacheAsync();
        }

        public async Task<List<BlockLiteDto>> GetBlockLitesAsync()
        {
            return (await GetCacheAsync()).Select(x => new BlockLiteDto(x)).ToList();
        }

        public async Task<TransactionDto> GetTransactionAsync(string hash)
        {
            var cache = await GetCacheAsync();

            return cache.Select(x => x.Transactions.FirstOrDefault(y => y.Hash == hash))
                        .FirstOrDefault(x => x != null);
        }

        public Task<CachedAddressDto> GetAddressAsync(string hash)
        {
            return GetCachedAddressAsync(hash);
        }

        public Task<List<CachedAddressDto>> GetAddressesAsync()
        {
            return GetAddressCacheAsync();
        }

        public async Task<List<AddressTransactionDto>> GetAddressTransactions(string hash)
        {
            var address = await GetCachedAddressAsync(hash);

            return address?.AddressTransactions
                ?? new List<AddressTransactionDto>();
        }

        public async Task<int> GetCacheSizeAsync()
        {
            return (await GetCacheAsync()).Count;
        }

        public async Task<int> GetCacheHeightAsync()
        {
            return await _redisCommand.GetAsync<int>(Settings.Redis.CachedHeight);
        }

        public async Task<int> GetChannelHeightAsync(BlockChannels channel, int height = 0)
        {
            var cache = await GetCacheAsync();

            return height == 0 
                ? cache.Count(x => x.Channel == (int)channel) 
                : cache.Count(x => x.Channel == (int)channel && x.Height <= height);
        }

        public async Task<double> GetChannelDifficultyAsync(BlockChannels channel)
        {
            var cache = await GetCacheAsync();

            return cache.LastOrDefault(x => x.Channel == (int)channel)?.Difficulty ?? 0;
        }

        public async Task<double> GetPosBlockRewardAsync()
        {
            var cache = await GetCacheAsync();

            return 0;
        }

        private async Task<List<BlockDto>> GetCacheAsync()
        {
            if (_cache == null || _cache.Count == 0)
                _cache = await SetCacheAsync();

            return _cache;
        }

        private async Task<List<BlockDto>> SetCacheAsync()
        {
            var cache = new List<BlockDto>();

            var cacheHeight = await _redisCommand.GetAsync<int?>(Settings.Redis.CachedHeight);

            if (!cacheHeight.HasValue || cacheHeight == 0)
            {
                _logger.LogWarning($"Cannot retrieve the cache height");
                return cache;
            }

            _logger.LogInformation($"Rebuilding local cache from block {cacheHeight}");

            for (var i = 0; i < Settings.App.BlockCacheCount; i++)
                cache.Add(await _redisCommand.GetAsync<BlockDto>(Settings.Redis.BuildCachedBlockKey(cacheHeight.Value - i)));

            _logger.LogInformation($"Local block cache rebuilt");

            return cache;
        }

        private async Task AddToCache(BlockLiteDto blockDto)
        {
            _cache.Insert(0, await _redisCommand.GetAsync<BlockDto>(Settings.Redis.BuildCachedBlockKey(blockDto.Height)));
            _cache.RemoveAt(_cache.Count - 1);

            _logger.LogInformation($"Added new block {blockDto.Height} to the local block cache");
        }

        private async Task<List<CachedAddressDto>> GetAddressCacheAsync()
        {
            var cache = await _redisCommand.GetAsync<List<CachedAddressDto>>(Settings.Redis.AddressCache);

            return cache ?? new List<CachedAddressDto>();
        }

        private Task<CachedAddressDto> GetCachedAddressAsync(string hash)
        { 
            return _redisCommand.GetAsync<CachedAddressDto>(Settings.Redis.BuildCachedAddressKey(hash));
        }

        private Task<List<BlockLiteDto>> GetBlockLiteCacheAsync()
        {
            return _redisCommand.GetAsync<List<BlockLiteDto>>(Settings.Redis.BlockLiteCache);
        }

        private Task<List<TransactionLiteDto>> GetTransactionLiteCacheAsync()
        {
            return _redisCommand.GetAsync<List<TransactionLiteDto>>(Settings.Redis.TransactionLiteCache);
        }
    }
}