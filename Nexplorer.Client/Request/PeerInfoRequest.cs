using Nexplorer.Client.Core;

namespace Nexplorer.Client.Request
{
    public class PeerInfoRequest : BaseRequest
    {
        public PeerInfoRequest() : base(0, "getpeerinfo") { }
    }
}