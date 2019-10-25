using Nexplorer.Connect.Hub.Core;

namespace Nexplorer.Connect.Nexus.Invoke
{
    public class GetBlocksInvoke : IHubInvoke
    {
        public string Name => "GetBlocks";
        public object[] Args { get; }

        public GetBlocksInvoke(int height, int count)
        {
            Args = new object[] {height, count};
        }
    }
}