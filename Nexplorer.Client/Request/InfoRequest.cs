using Nexplorer.Client.Core;

namespace Nexplorer.Client.Request
{
    public class InfoRequest : BaseRequest
    {
        public InfoRequest() : base(0, "getinfo") { }
    }
}
