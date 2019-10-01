using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexplorer.Nexus;
using Nexplorer.Nexus.Assets;
using Nexplorer.Nexus.Ledger;
using Nexplorer.Nexus.Tokens;

namespace Nexplorer.Core
{
    public static class StartupExtensions
    {
        public static IConfigurationBuilder AddApplicationPath(this IConfigurationBuilder config, string environment)
        {
            if (!string.IsNullOrEmpty(environment))
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
            }

            return config;
        }

        public static IConfigurationBuilder AddApplicationSettings(this IConfigurationBuilder config, string environment)
        {
            config.AddJsonFile("appsettings.json", false, true);

            if (!string.IsNullOrEmpty(environment) && environment != "Local")
            {
                config.AddJsonFile($"appsettings.{environment}.json", false, true);
            }

            return config;
        }      
        
        public static void AddNexusServices(this IServiceCollection services, NexusNodeEndpoint endpoint)
        {
            services.AddHttpClient<INexusClient, NexusClient>();
            services.AddSingleton(endpoint);
            services.AddSingleton<INexusConnection, NexusConnection>();

            services.AddTransient<ILedgerService, LedgerService>();
            services.AddTransient<IAssetService, AssetService>();
            services.AddTransient<ITokenService, TokenService>();

            services.AddHostedService<HealthCheckService>();
        }
    }
}