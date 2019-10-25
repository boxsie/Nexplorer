namespace Nexplorer.Connect.Hub.Core
{
    public interface IHubInvoke
    {
        string Name { get; }
        object[] Args { get; }
    }
}