using Nexplorer.Connect.Hub.Core;

namespace Nexplorer.Connect.Nexus.Invoke
{
    public class GetBlockInvoke : IHubInvoke
    {
        public string Name => "GetBlock";
        public object[] Args { get; }

        public GetBlockInvoke(int height)
        {
            Args = new object[] { height };
        }
    }
}