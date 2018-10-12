using Nexplorer.Client.Core;

namespace Nexplorer.Client.Request
{
    public class TrustKeysRequest : BaseRequest
    {
        public TrustKeysRequest() : base(0, "getnetworktrustkeys") { }
    }
}