using System.Threading;
using System.Threading.Tasks;

namespace Nexplorer.Nexus
{
    public interface INexusService
    {
        Task<T> GetAsync<T>(string path, NexusRequest request, CancellationToken token = default, bool logOutput = true);
    }
}