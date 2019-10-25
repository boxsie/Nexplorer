using Nexplorer.Connect.Hub;
using Nexplorer.Data;

namespace Nexplorer.Connect
{
    public class AppSettings
    {
        public NexusDbSettings NexusDbSettings { get; set; }
        public HubSettings[] Hubs { get; set; }
    }
}