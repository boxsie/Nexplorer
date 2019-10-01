using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexplorer.Core;

namespace Nexplorer.Connect
{
    public static class Program
    {
        public static string CurrentEnvironment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        public static async Task Main(string[] args)
        {
            var startup = new Startup();

            var host = new HostBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddApplicationPath(CurrentEnvironment);
                    config.AddApplicationSettings(CurrentEnvironment);
                    config.AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(logging => { logging.AddConsole(); })
                        .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

                    startup.ConfigureServices(context, services);
                })
                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();
        }
    }
}
