using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Core
{
    public interface INexplorer
    {
        Task PublishNewHeight(int height);
        Task ReceiveBlocks(IEnumerable<Block> blocks);
        Task ReceiveBlock(Block block);
    }

    public class NexplorerMsg<T>
    {
        public T Result { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string[] Error { get; set; }
        public long ResponseTimeMs { get; set; }
    }
}