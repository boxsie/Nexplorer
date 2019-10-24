namespace Nexplorer.Connect
{
    public interface IHubInvoke
    {
        string Name { get; }
        object[] Args { get; }
    }
}