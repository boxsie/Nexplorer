using System.Collections.Generic;
using System.Threading.Tasks;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Data
{
    public interface IBlockDb
    {
        Task<List<Block>> GetAsync();
        Task<Block> GetAsync(int height);
        Task<Block> GetHighestAsync();
        Task<Block> CreateAsync(Block block);
        Task CreateManyAsync(IEnumerable<Block> blocks);
        Task UpdateAsync(int height, Block blockIn);
        Task RemoveAsync(Block blockIn);
        Task RemoveAsync(int height);
    }
}