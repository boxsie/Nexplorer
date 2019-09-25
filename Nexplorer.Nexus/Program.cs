using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexplorer.Nexus.Ledger;
using Nexplorer.Nexus.Nexus;

namespace Nexplorer.Nexus
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var services = ConfigureServices();

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<Startup>().Run(serviceProvider.GetService<ILedgerService>());
        }

        private static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddLogging(logging => { logging.AddConsole(); })
                    .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

            services.AddSingleton<Startup>();
            services.AddHttpClient<INexusClient, NexusClient>();

            services.AddSingleton(new NexusNodeEndpoint
            {
                Url = "http://localhost:8080/;",
                Username = "username",
                Password = "password",
                ApiSessions = true,
                IndexHeight = true
            });

            services.AddTransient<NexusNode>();
            services.AddTransient<ILedgerService, LedgerService>();

            return services;
        }
    }

    internal class Startup
    {
        public void Run(ILedgerService ls)
        {
            Console.WriteLine(ls.GetBlockAsync(1).GetAwaiter().GetResult());
            Console.Read();
        }
    }

    public class NexusServiceFactory : INexusServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public NexusServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Get<T>(NexusNodeEndpoint nodeEndpoint) where T : NexusService
        {
            var nexusNode = new NexusNode(_serviceProvider.GetService<INexusClient>(), nodeEndpoint);

            var service = Activator.CreateInstance(typeof(T), nexusNode, _serviceProvider.GetService<ILogger<NexusService>>());

            return (T)service;
        }
    }

    public interface INexusServiceFactory
    {
        T Get<T>(NexusNodeEndpoint nodeEndpoint) where T : NexusService;
    }
}
