using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexplorer.NexusClient.Core;
using Nexplorer.NexusClient.Response;

namespace Nexplorer.NexusClient
{
    public class NexusBlockStream
    {
        private readonly INxsClient _nxsClient;
        private readonly Dictionary<Guid, Func<BlockResponse, Task>> _subscribers;
        private readonly CancellationTokenSource _cancelBlockStream;

        private string _lastHash;

        public NexusBlockStream(INxsClient nxsClient)
        {
            _nxsClient = nxsClient;

            _subscribers = new Dictionary<Guid, Func<BlockResponse, Task>>();
            _cancelBlockStream = new CancellationTokenSource();
        }

        public async Task Start(TimeSpan checkDelay)
        {
            _lastHash = await _nxsClient.GetBlockHashAsync(await _nxsClient.GetBlockCountAsync());

#pragma warning disable 4014
            Task.Run(() => StreamAsync(checkDelay), _cancelBlockStream.Token);
#pragma warning restore 4014
        }

        public void Stop()
        {
            _cancelBlockStream.Cancel();
        }

        public Guid Subscribe(Func<BlockResponse, Task> onNewBlock)
        {
            var guid = Guid.NewGuid();

            _subscribers.Add(guid, onNewBlock);

            return guid;
        }

        public void Unsubscribe(Guid id)
        {
            if (_subscribers.ContainsKey(id))
                _subscribers.Remove(id);
        }

        public void Reset()
        {
            _subscribers.Clear();

            Stop();
        }

        private async Task StreamAsync(TimeSpan checkDelay)
        {
            while (true)
            {
                try
                {
                    _cancelBlockStream.Token.ThrowIfCancellationRequested();

                    var block = await _nxsClient.GetNextBlockAsync(_lastHash);

                    if (block != null)
                    {
                        foreach (var subscriber in _subscribers.Values)
                            await subscriber(block);

                        _lastHash = block.Hash;
                    }

                    await Task.Delay(checkDelay);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}