using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Connect
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

    public class GetBlocksInvoke : IHubInvoke
    {
        public string Name => "GetBlocks";
        public object[] Args { get; }

        public GetBlocksInvoke(int height, int count)
        {
            Args = new object[] {height, count};
        }
    }

    public interface IHubInvoke
    {
        string Name { get; }
        object[] Args { get; }
    }
}