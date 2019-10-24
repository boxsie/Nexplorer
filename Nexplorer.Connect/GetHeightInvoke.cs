namespace Nexplorer.Connect
{
    public class GetHeightInvoke : IHubInvoke
    {
        public string Name => "GetHeight";
        public object[] Args => new object[0];
    }
}