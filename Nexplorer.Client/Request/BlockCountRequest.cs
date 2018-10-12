using Nexplorer.Client.Core;

namespace Nexplorer.Client.Request
{
    public class BlockCountRequest : BaseRequest
    {
        public BlockCountRequest() : base(0, "getblockcount") { }
    }
}