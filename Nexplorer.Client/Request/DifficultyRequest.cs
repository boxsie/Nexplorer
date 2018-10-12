using Nexplorer.Client.Core;

namespace Nexplorer.Client.Request
{
    public class DifficultyRequest : BaseRequest
    {
        public DifficultyRequest() : base(0, "getdifficulty") { }
    }
}