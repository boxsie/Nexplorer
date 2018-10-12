using Nexplorer.Client.Core;

namespace Nexplorer.Client.Request
{
    public class BlockHashRequest : BaseRequest
    {
        public int Height { get; set; }

        public BlockHashRequest(int height) : base(0, "getblockhash", height)
        {
            Height = height;
        }
    }
}