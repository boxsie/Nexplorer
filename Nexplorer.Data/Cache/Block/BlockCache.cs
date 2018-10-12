using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Data.Cache.Block
{
    public class BlockCache : IBlockCache
    {
        private List<BlockCacheItem<BlockDto>> _cache;
        private List<BlockLiteDto> _blockLiteCache;
        private List<TransactionLiteDto> _transactionLiteCache;
        private Dictionary<string, CachedAddressDto> _addressCache;

        private readonly RedisCommand _redisCommand;
        private readonly BlockQuery _blockQuery;

        public BlockCache(RedisCommand redisCommand, BlockQuery blockQuery)
        {
            _redisCommand = redisCommand;
            _blockQuery = blockQuery;
            _addressCache = new Dictionary<string, CachedAddressDto>();
        }

        public Task<int> GetBlockCacheHeightAsync()
        {
            if (_cache == null || _cache.Count == 0)
                return Task.FromResult(0);

            return Task.FromResult(_cache.Max(x => x.Item.Height));
        }

        public async Task<List<BlockDto>> GetBlockCacheAsync()
        {
            if (_cache != null)
                return _cache.Select(x => x.Item).ToList();

            _cache = new List<BlockCacheItem<BlockDto>>();

            var nextHeight = await _blockQuery.GetLastHeightAsync() + 1;

            for (var i = nextHeight; i < nextHeight + Settings.App.BlockCacheCount; i++)
            {
                var cacheBlock = await _redisCommand.GetAsync<BlockDto>(Settings.Redis.BuildCachedBlockKey(i));

                if (cacheBlock == null)
                    break;

                _cache.Add(new BlockCacheItem<BlockDto>(cacheBlock));
            }

            return _cache.Select(x => x.Item).ToList();
        }

        public async Task<List<BlockLiteDto>> GetBlockLiteCacheAsync()
        {
            if (_blockLiteCache != null)
                return _blockLiteCache;

            _blockLiteCache = await _redisCommand.GetAsync<List<BlockLiteDto>>(Settings.Redis.BlockLiteCache);

            return _blockLiteCache;
        }

        public async Task<List<TransactionLiteDto>> GetTransactionLiteCacheAsync()
        {
            if (_transactionLiteCache != null)
                return _transactionLiteCache;

            _transactionLiteCache = await _redisCommand.GetAsync<List<TransactionLiteDto>>(Settings.Redis.TransactionLiteCache);

            return _transactionLiteCache;
        }

        public async Task<List<CachedAddressDto>> GetCachedAddressAsync()
        {
            if (_addressCache != null)
                return _addressCache.Values.ToList();

            _addressCache = (await _redisCommand.GetAsync<List<CachedAddressDto>>(Settings.Redis.AddressCache))?.ToDictionary(x => x.Hash);

            return _addressCache?.Values.ToList();
        }

        public Task AddAsync(BlockDto block)
        {
            if (_cache == null)
                _cache = new List<BlockCacheItem<BlockDto>>();

            _cache.Insert(0, new BlockCacheItem<BlockDto>(block, true));
            CheckBlockForAddress(block);

            AddLiteBlock(block);
            AddLiteTransactions(block.Height, block.Transactions);

            return Task.CompletedTask;
        }
        
        public async Task RemoveAllBelowAsync(int height)
        {
            var heightsToRemove = _cache
                .Select(x => x.Item)
                .Where(x => x.Height <= height)
                .Select(x => x.Height);

            foreach (var i in heightsToRemove)
                await _redisCommand.DeleteAsync(Settings.Redis.BuildCachedBlockKey(i));

            _cache.RemoveAll(x => x.Item.Height <= height);
            _blockLiteCache.RemoveAll(x => x.Height <= height);
            _transactionLiteCache.RemoveAll(x => x.BlockHeight <= height);

            var keys = _addressCache.Keys.ToList();

            foreach (var addressHash in keys)
            {
                if (_cache.Any(x => x.Item.Transactions
                             .Any(y => y.Inputs.Concat(y.Outputs)
                                .Any(z => z.AddressHash == addressHash))))
                    continue;

                await RemoveCachedAddress(addressHash);
            }
        }

        public async Task RemoveCachedAddress(string hash)
        {
            if (_addressCache.ContainsKey(hash))
            {
                _addressCache.Remove(hash);

                await _redisCommand.DeleteAsync(Settings.Redis.BuildCachedAddressKey(hash));
            }
        }

        public Task UpdateTransactionsAsync(List<BlockCacheTransaction> txUpdates)
        {
            foreach (var txUpdate in txUpdates)
            {
                var txItem = _cache.FirstOrDefault(x => x.Item.Height == txUpdate.Height);

                var tx = txItem?.Item.Transactions.FirstOrDefault(x => x.Hash == txUpdate.TxHash);

                if (tx != null && tx.Confirmations != txUpdate.TransactionUpdate.Confirmations)
                {
                    tx.Confirmations = txUpdate.TransactionUpdate.Confirmations;
                    txItem.NeedsUpdate = true;
                }

                var txLite = _transactionLiteCache.FirstOrDefault(x => x.TransactionHash == txUpdate.TxHash);

                if (txLite != null)
                    txLite.Confirmations = txUpdate.TransactionUpdate.Confirmations;
            }

            return Task.CompletedTask;
        }
        
        public async Task SaveAsync()
        {
            foreach (var blockDto in _cache.Where(x => x.NeedsUpdate).Select(x => x.Item).ToList())
                await _redisCommand.SetAsync(Settings.Redis.BuildCachedBlockKey(blockDto.Height), blockDto);

            await _redisCommand.SetAsync(Settings.Redis.BlockLiteCache, _blockLiteCache);
            await _redisCommand.SetAsync(Settings.Redis.TransactionLiteCache, _transactionLiteCache);

            var addressValues = _addressCache.Values.ToList();

            foreach (var addressDto in addressValues)
                await _redisCommand.SetAsync(Settings.Redis.BuildCachedAddressKey(addressDto.Hash), addressDto);
            
            await _redisCommand.SetAsync(Settings.Redis.AddressCache, addressValues);
        }

        public Task Clear()
        {
            _cache = new List<BlockCacheItem<BlockDto>>();
            _blockLiteCache = new List<BlockLiteDto>();
            _transactionLiteCache = new List<TransactionLiteDto>();

            return Task.CompletedTask;
        }

        public async Task<bool> BlockExistsAsync(int height)
        {
            return (await GetBlockCacheAsync())?.Any(x => x.Height == height) ?? false;
        }

        private void AddLiteBlock(BlockDto block)
        {
            if (_blockLiteCache == null)
                _blockLiteCache = new List<BlockLiteDto>();

            var blockLite = new BlockLiteDto(block);

            _blockLiteCache.Insert(0, blockLite);

            if (_blockLiteCache.Count > Settings.App.BlockLiteCacheCount)
                _blockLiteCache.RemoveRange(Settings.App.BlockLiteCacheCount, _blockLiteCache.Count - Settings.App.BlockLiteCacheCount);
        }

        private void AddLiteTransactions(int blockHeight, IEnumerable<TransactionDto> txs)
        {
            if (_transactionLiteCache == null)
                _transactionLiteCache = new List<TransactionLiteDto>();

            var txLites = txs.Select(x => new TransactionLiteDto(x, blockHeight)).ToList();

            _transactionLiteCache.InsertRange(0, txLites);

            if (_transactionLiteCache.Count > Settings.App.TransactionLiteCacheCount)
                _transactionLiteCache.RemoveRange(Settings.App.TransactionLiteCacheCount, _transactionLiteCache.Count - Settings.App.TransactionLiteCacheCount);
        }

        private void CheckBlockForAddress(BlockDto block)
        {
            foreach (var txDto in block.Transactions)
            {
                foreach (var txIn in txDto.Inputs)
                    AddAddressTransaction(txIn.AddressHash, block.Height, txDto.TimeUtc, txDto.Hash, txIn.Amount, TransactionType.Input);

                foreach (var txOut in txDto.Outputs)
                    AddAddressTransaction(txOut.AddressHash, block.Height, txDto.TimeUtc, txDto.Hash, txOut.Amount, TransactionType.Output);
            }
        }

        private void AddAddressTransaction(string addressHash, int blockHeight, DateTime date, string txHash, double amount, TransactionType txType)
        {
            var existingAddress = _addressCache.ContainsKey(addressHash);

            var address = existingAddress
                ? _addressCache[addressHash]
                : new CachedAddressDto
                {
                    Hash = addressHash,
                    FirstBlockHeight = blockHeight,
                    Aggregate = new AddressAggregateDto(),
                    AddressTransactions = new List<AddressTransactionDto>()
                };

            address.Aggregate.ModifyAggregateProperties(txType, amount, blockHeight);

            address.AddressTransactions.Add(new AddressTransactionDto
            {
                BlockHeight = blockHeight,
                TimeUtc = date,
                TransactionHash = txHash,
                Amount = amount,
                TxType = txType
            });

            if (!existingAddress)
                _addressCache.Add(addressHash, address);
        }
    }
}
