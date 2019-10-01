using System;
using System.Threading.Tasks;

namespace Nexplorer.Connect
{
    public interface IHubEvent<in T>
    {
        string Name { get; }

        Task Handle(T obj);
    }
}