using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nexplorer.Nexus.Nexus
{
    public interface INexusClient
    {
        void ConfigureHttpClient(NexusNodeEndpoint endpoint);
        Task<HttpResponseMessage> GetAsync(string path, string logHeader, NexusRequest request, CancellationToken token, bool logOutput);
        Task<HttpResponseMessage> PostAsync(string path, string logHeader, NexusRequest request, CancellationToken token, bool logOutput);
    }
}