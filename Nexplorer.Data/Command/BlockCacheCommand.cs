using System.Collections.Generic;
using System.Threading.Tasks;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Data.Command
{
    public class BlockCacheCommand
    {
        private readonly RedisCommand _redisCommand;
        private readonly BlockQuery _blockQuery;

        public BlockCacheCommand(RedisCommand redisCommand, BlockQuery blockQuery)
        {
            _redisCommand = redisCommand;
            _blockQuery = blockQuery;
        }

        public async Task BuildAsync()
        {
            var last = await _blockQuery.GetLastBlockAsync();
            var cache = new List<BlockDto>();

            while (cache.Count < Settings.App.BlockCacheSize)
            {
                if (last == null)
                    break;

                cache.Add(last);

                last = await _blockQuery.GetBlockAsync(last.Height - 1, true);
            }

            foreach (var blockDto in cache)
                await _redisCommand.SetAsync(Settings.Redis.BuildBlockCacheKey(blockDto.Height), blockDto);
        }

        public async Task AddAsync(BlockDto block)
        {
            if (block == null)
                return;

            await _redisCommand.SetAsync(Settings.Redis.BuildBlockCacheKey(block.Height), block);
            await _redisCommand.SetAsync(Settings.Redis.ChainHeight, block.Height);
            await _redisCommand.DeleteAsync(Settings.Redis.BuildBlockCacheKey(block.Height - Settings.App.BlockCacheSize));
        }

        public async Task<List<BlockDto>> GetAsync()
        {
            var chainHeight = await _redisCommand.GetAsync<int>(Settings.Redis.ChainHeight);

            var blocks = new List<BlockDto>();

            if (chainHeight == 0)
                return blocks;

            for (var i = chainHeight; i >= chainHeight - Settings.App.BlockCacheSize; i--)
                blocks.Add(await _redisCommand.GetAsync<BlockDto>(Settings.Redis.BuildBlockCacheKey(i)));

            return blocks;
        }
    }
}