using Nexplorer.Connect.Hub.Core;

namespace Nexplorer.Connect.Nexus.Invoke
{
    public class GetHeightInvoke : IHubInvoke
    {
        public string Name => "GetHeight";
        public object[] Args => new object[0];
    }
}