using System;
using System.Threading.Tasks;
using Nexplorer.Connect.Hub.Core;

namespace Nexplorer.Connect.Hub
{
    public class HubEvent<T> : IHubEvent<T>
    {
        public string Name { get; }

        private readonly Func<T, Task> _onResponseAsync;

        public HubEvent(string name, Func<T, Task> onResponseAsync)
        {
            Name = name;
            _onResponseAsync = onResponseAsync;
        }

        public Task Handle(T obj)
        {
            return _onResponseAsync(obj);
        }
    }
}