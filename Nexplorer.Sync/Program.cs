using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexplorer.Client;
using Nexplorer.Client.Core;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Cache.Block;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Data.Command;
using Nexplorer.Data.Context;
using Nexplorer.Data.Map;
using Nexplorer.Data.Publish;
using Nexplorer.Data.Query;
using Nexplorer.Infrastructure.Bittrex;
using Nexplorer.Infrastructure.Geolocate;
using Nexplorer.Sync.Nexus;
using NLog.Extensions.Logging;
using StackExchange.Redis;
using Nexplorer.Sync.Jobs;
using NLog;
using BlockCache = Nexplorer.Sync.Jobs.BlockCache;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Nexplorer.Sync
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);
            
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Attach Config
            Settings.AttachConfig(serviceProvider);
            
            // Configure NLog
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            LogManager.LoadConfiguration("nlog.config");
            
            // Clear Redis
            var endpoints = serviceProvider.GetService<ConnectionMultiplexer>().GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = serviceProvider.GetService<ConnectionMultiplexer>().GetServer(endpoint);
                server.FlushAllDatabases();
            }

            // Migrate EF
            serviceProvider.GetService<NexusDb>().Database.Migrate();

            // Run app
            serviceProvider.GetService<App>().Run().GetAwaiter().GetResult();

            Console.Read();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace));

            var configuration = Settings.BuildConfig(services);
                
            services.AddDbContext<NexusDb>(x => x.UseMySql(configuration.GetConnectionString("NexusDb"), y => y.MigrationsAssembly("Nexplorer.Data")), ServiceLifetime.Transient);

            services.AddSingleton(ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));

            services.AddSingleton<AutoMapperConfig>();
            services.AddSingleton(x => x.GetService<AutoMapperConfig>().GetMapper());

            services.AddSingleton<IBlockCache, Data.Cache.Block.BlockCache>();
            services.AddSingleton<BlockCacheService>();
            services.AddSingleton<GeolocationService>();

            services.AddTransient<NexusQuery>();
            services.AddTransient<BlockQuery>();
            services.AddTransient<AddressQuery>();
            services.AddTransient<StatQuery>();

            services.AddTransient<BlockCacheBuild>();
            services.AddTransient<LatestBlockPublisher>();
            services.AddTransient<RollingCountPublisher>();
            services.AddTransient<RedisCommand>();
            services.AddTransient<BlockAddCommand>();
            services.AddTransient<AddressAggregateUpdateCommand>();
            services.AddTransient<TransactionInputOutputMapper>();

            services.AddTransient<BlockSyncCatchup>();
            services.AddTransient<BlockSync>();
            services.AddTransient<BlockScan>();
            services.AddTransient<BlockCache>();
            services.AddTransient<BittrexSync>();
            services.AddTransient<MiningStats>();
            services.AddTransient<AddressStats>();
            services.AddTransient<AddressCache>();
            services.AddTransient<AddressAggregation>();
            services.AddTransient<TransactionRefresh>();

            services.AddTransient<INexusClient, NexusClient>();
            services.AddTransient<INexusClient, NexusClient>(x => new NexusClient(x.GetService<ILogger<RpcClient>>(), configuration.GetConnectionString("Nexus")));
            services.AddTransient<BittrexClient>();
            services.AddTransient<GeolocateIpClient>();
            
            services.AddTransient<App>();
        }
    }
}
