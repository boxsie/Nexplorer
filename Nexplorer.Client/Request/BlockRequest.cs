using Nexplorer.Client.Core;

namespace Nexplorer.Client.Request
{
    public class BlockRequest : BaseRequest
    {
        public string BlockHash { get; }

        public BlockRequest(string blockHash) : base(0, "getblock", blockHash)
        {
            BlockHash = blockHash;
        }
    }
}