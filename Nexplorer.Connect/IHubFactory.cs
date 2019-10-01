namespace Nexplorer.Connect
{
    public interface IHubFactory
    {
        IHubClient Get(string name);
    }
}