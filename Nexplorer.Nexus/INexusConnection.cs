using System.Threading;
using System.Threading.Tasks;

namespace Nexplorer.Nexus
{
    public interface INexusConnection
    {
        bool Available { get; }

        Task RefreshAsync();
        Task<NexusResponse<T>> GetAsync<T>(string path, NexusRequest request, CancellationToken token = default);
        Task<NexusResponse<T>> PostAsync<T>(string path, NexusRequest request, CancellationToken token = default);
    }
}