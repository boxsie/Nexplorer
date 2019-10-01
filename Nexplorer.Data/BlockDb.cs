using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Data
{
    public class BlockDb : IBlockDb
    {
        private readonly ILogger<BlockDb> _logger;
        private readonly IMongoCollection<Block> _blocks;

        public BlockDb(NexusDbSettings settings, IMongoClient client, ILogger<BlockDb> logger)
        {
            _logger = logger;
            var database = client.GetDatabase(settings.DatabaseName);

            _blocks = database.GetCollection<Block>(settings.BlockCollectionName);
        }

        public async Task<List<Block>> GetAsync()
        {
            return (await _blocks.FindAsync(block => true)).ToList();
        }

        public Task<Block> GetAsync(int height)
        {
            return _blocks
                .Find(block => block.Height == height)
                .FirstOrDefaultAsync();
        }

        public Task<Block> GetHighestAsync()
        {
            return _blocks
                .Find(block => true)
                .SortByDescending(block => block.Height)
                .Limit(1)
                .FirstOrDefaultAsync();
        }

        public async Task<Block> CreateAsync(Block block)
        {
            try
            {
                await _blocks.InsertOneAsync(block);
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to create block {block.Height} record");
                _logger.LogError(e.Message);
                if (e.InnerException != null)
                    _logger.LogError(e.InnerException.Message);
            }

            return block;
        }

        public async Task CreateManyAsync(IEnumerable<Block> blocks)
        {
            try
            {
                await _blocks.InsertManyAsync(blocks);
            }
            catch (Exception e)
            {
                _logger.LogError("Unable to create blocks records");
                _logger.LogError(e.Message);
                if (e.InnerException != null)
                    _logger.LogError(e.InnerException.Message);
            }
        }

        public Task UpdateAsync(int height, Block blockIn)
        {
            return _blocks.ReplaceOneAsync(block => block.Height == height, blockIn);
        }

        public Task RemoveAsync(Block blockIn)
        {
            return _blocks.DeleteOneAsync(block => block.Height == blockIn.Height);
        }

        public Task RemoveAsync(int height)
        {
            return _blocks.DeleteOneAsync(block => block.Height == height);
        }
    }
}