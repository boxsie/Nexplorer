using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Command;
using Nexplorer.Data.Context;
using Nexplorer.Data.Map;
using Nexplorer.Data.Query;
using Nexplorer.Data.Services;
using Nexplorer.Infrastructure.Bittrex;
using Nexplorer.Infrastructure.Geolocate;
using Nexplorer.Jobs;
using Nexplorer.Jobs.Catchup;
using Nexplorer.Jobs.Service;
using Nexplorer.NexusClient;
using Nexplorer.NexusClient.Core;
using NLog.Extensions.Logging;
using StackExchange.Redis;
using NLog;
using NLog.Config;
using NLog.Targets;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Nexplorer.Sync
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.AddNLog(new NLogProviderOptions
                    {
                        CaptureMessageTemplates = true,
                        CaptureMessageProperties = true
                    });
                });

            ConfigureServices(serviceCollection);
            
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Attach Config
            Settings.AttachConfig(serviceProvider);

            // Clear Redis
            var endpoints = serviceProvider.GetService<ConnectionMultiplexer>().GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var redis = serviceProvider.GetService<ConnectionMultiplexer>().GetServer(endpoint);
                redis.FlushAllDatabases();
            }

            // Set up Nlog
            ConfigurationItemFactory.Default.Targets.RegisterDefinition("RedisTarget", typeof(RedisTarget));
            ConfigurationItemFactory.Default.CreateInstance = 
                (Type type) => type == typeof(RedisTarget) 
                    ? serviceProvider.GetService<RedisTarget>() 
                    : Activator.CreateInstance(type);

            // Migrate EF
            serviceProvider.GetService<NexusDb>().Database.Migrate();

            JobService.Start(serviceProvider.GetService<IEnumerable<IHostedService>>());

            Task.Run(serviceProvider.GetService<App>().StartAsync);

            Console.Read();

            NLog.LogManager.Shutdown();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var configuration = Settings.BuildConfig(services);

            services.AddDbContext<NexusDb>(x => x.UseSqlServer(configuration.GetConnectionString("NexusDb"), y => { y.MigrationsAssembly("Nexplorer.Data"); }), ServiceLifetime.Transient);

            services.AddSingleton(ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));
            services.AddSingleton<RedisCommand>();

            services.AddSingleton<AutoMapperConfig>();
            services.AddSingleton(x => x.GetService<AutoMapperConfig>().GetMapper());

            services.AddTransient<RedisTarget>();
            
            services.AddSingleton<GeolocationService>();

            services.AddScoped<BlockInsertCommand>();
            services.AddScoped<BlockDeleteCommand>();
            services.AddScoped<BlockPublishCommand>();
            services.AddScoped<AddressAggregatorCommand>();
            services.AddScoped<BlockCacheCommand>();

            services.AddScoped<NexusQuery>();
            services.AddScoped<BlockQuery>();
            services.AddScoped<AddressQuery>();
            services.AddScoped<StatQuery>();

            services.AddScoped<INxsClient, NxsClient>();
            services.AddScoped<INxsClient, NxsClient>(x => new NxsClient(configuration.GetConnectionString("Nexus")));
            services.AddScoped<NexusBlockStream>();
            services.AddScoped<BittrexClient>();
            services.AddScoped<GeolocateIpClient>();

            services.AddSingleton<App>();

            services.AddSingleton<BlockSyncCatchup>();
            services.AddSingleton<AddressAggregateCatchup>();
            services.AddSingleton<BlockRewardCatchup>();

            JobService.Init(services);
        }
    }

    [Target("Redis")]
    public sealed class RedisTarget : TargetWithLayout
    {
        private readonly RedisCommand _redisCommand;

        public RedisTarget(RedisCommand redisCommand)
        {
            _redisCommand = redisCommand;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var logMessage = this.Layout.Render(logEvent);

            _redisCommand.Publish(Settings.Redis.SyncOutputPubSub, logMessage);
        }
    }
}
