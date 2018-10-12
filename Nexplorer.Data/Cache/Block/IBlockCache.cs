using System.Collections.Generic;
using System.Threading.Tasks;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Data.Cache.Block
{
    public interface IBlockCache
    {
        Task<int> GetBlockCacheHeightAsync();
        Task<List<BlockDto>> GetBlockCacheAsync();
        Task<List<BlockLiteDto>> GetBlockLiteCacheAsync();
        Task<List<TransactionLiteDto>> GetTransactionLiteCacheAsync();

        Task AddAsync(BlockDto block);
        Task RemoveAllBelowAsync(int height);
        Task UpdateTransactionsAsync(List<BlockCacheTransaction> txUpdates);
        Task SaveAsync();
        Task Clear();
        Task<bool> BlockExistsAsync(int height);
    }
}