using Nexplorer.Client.Core;

namespace Nexplorer.Client.Request
{
    public class MiningInfoRequest : BaseRequest
    {
        public MiningInfoRequest() : base(0, "getmininginfo") { }
    }
}