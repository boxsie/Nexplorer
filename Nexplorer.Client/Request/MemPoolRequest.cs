using Nexplorer.Client.Core;

namespace Nexplorer.Client.Requests
{
    public class MemPoolRequest : BaseRequest
    {
        public MemPoolRequest() : base(0, "getrawmempool") { }
    }
}