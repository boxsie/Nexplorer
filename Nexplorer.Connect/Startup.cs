using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nexplorer.Connect.Hub;
using Nexplorer.Connect.Hub.Core;
using Nexplorer.Connect.Nexus;
using Nexplorer.Data;

namespace Nexplorer.Connect
{
    public class Startup
    {
        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var appSettings = context.Configuration.Get<AppSettings>();
            services.AddSingleton(appSettings);

            services.AddDataServices(appSettings.NexusDbSettings);

            services.AddSingleton<IHubFactory, HubFactory>();
            services.AddSingleton<IHostedService, NexusSyncService>();
            services.AddSingleton<INexusHubService, NexusHubService>();
        }
    }
}