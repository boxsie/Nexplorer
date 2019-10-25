using System.Threading.Tasks;

namespace Nexplorer.Connect.Hub.Core
{
    public interface IHubEvent<in T>
    {
        string Name { get; }

        Task Handle(T obj);
    }
}