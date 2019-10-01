using System.Collections.Generic;
using System.Threading.Tasks;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Core
{
    public interface INexplorer
    {
        Task PublishNewHeight(int height);
        Task ReceiveBlocks(IEnumerable<Block> blocks);
    }
}