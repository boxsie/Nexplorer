namespace Nexplorer.Connect.Hub.Core
{
    public interface IHubFactory
    {
        IHubClient Get(string name);
    }
}