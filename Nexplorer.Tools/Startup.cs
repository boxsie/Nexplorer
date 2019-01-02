using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Data.Command;
using Nexplorer.Data.Context;
using Nexplorer.Data.Map;
using Nexplorer.Data.Query;
using Nexplorer.Infrastructure.Bittrex;
using Nexplorer.NexusClient;
using Nexplorer.NexusClient.Core;
using Nexplorer.Tools.Jobs;
using Nexplorer.Tools.Jobs.Catchup;
using StackExchange.Redis;

namespace Nexplorer.Tools
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace));

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddCors();

            var config = Settings.BuildConfig(services);

            services.AddDbContext<NexusDb>(x => x.UseSqlServer(config.GetConnectionString("NexusDb"), y => y.MigrationsAssembly("Nexplorer.Data")), ServiceLifetime.Scoped);
            services.AddDbContext<NexplorerDb>(x => x.UseSqlServer(config.GetConnectionString("NexplorerDb"), y => y.MigrationsAssembly("Nexplorer.Data")), ServiceLifetime.Scoped);

            services.AddSingleton(ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")));
            services.AddSingleton<RedisCommand>();

            services.AddSingleton<AutoMapperConfig>();
            services.AddSingleton(x => x.GetService<AutoMapperConfig>().GetMapper());

            services.AddHangfire(x => x.UseSqlServerStorage(config.GetConnectionString("NexplorerDb")));

            services.AddSingleton<BlockCacheService>();

            services.AddScoped<NexusQuery>();
            services.AddScoped<BlockQuery>();
            services.AddScoped<AddressQuery>();
            services.AddScoped<StatQuery>();
            services.AddScoped<AddressAggregator>();

            services.AddScoped<INxsClient, NxsClient>();
            services.AddScoped<INxsClient, NxsClient>(x => new NxsClient(config.GetConnectionString("Nexus")));
            services.AddScoped<NexusBlockStream>();
            services.AddScoped<BittrexClient>();

            services.AddScoped<App>();
            
            services.AddScoped<BlockSyncCatchup>();
            services.AddScoped<AddressAggregateCatchup>();
            services.AddScoped<BlockRewardCatchup>();
            services.AddScoped<JobsInit>();
            services.AddScoped<BlockScanJob>();
            services.AddScoped<BlockCacheJob>();
            services.AddScoped<BlockPublishJob>();
            services.AddScoped<BlockCacheCleanupJob>();
            services.AddScoped<TrustAddressCacheJob>();
            services.AddScoped<NexusAddressCacheJob>();
            services.AddScoped<AddressStatsJob>();
            services.AddScoped<ExchangeSyncJob>();
            services.AddScoped<MiningLatestJob>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            Settings.AttachConfig(serviceProvider);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            
            GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(serviceProvider));

            app.UseHangfireServer();
            app.UseHangfireDashboard();
            
            JobStorage.Current.GetMonitoringApi().PurgeJobs();

            // Clear Redis
            var endpoints = serviceProvider.GetService<ConnectionMultiplexer>().GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = serviceProvider.GetService<ConnectionMultiplexer>().GetServer(endpoint);
                server.FlushAllDatabases();
            }

            var toolsApp = serviceProvider.GetService<App>();
            Task.Run(async () => { await toolsApp.StartAsync(); });
        }
    }

    public class App
    {
        private readonly NexusQuery _nexusQuery;
        private readonly BlockSyncCatchup _blockCatchup;
        private readonly AddressAggregateCatchup _addressCatchup;
        private readonly BlockCacheJob _blockCacheJob;
        private readonly BlockCacheService _blockCache;
        private readonly RedisCommand _redisCommand;

        public App(NexusQuery nexusQuery, BlockSyncCatchup blockCatchup, AddressAggregateCatchup addressCatchup, 
            BlockCacheJob blockCacheJob, BlockCacheService blockCache, RedisCommand redisCommand)
        {
            _nexusQuery = nexusQuery;
            _blockCatchup = blockCatchup;
            _addressCatchup = addressCatchup;
            _blockCacheJob = blockCacheJob;
            _blockCache = blockCache;
            _redisCommand = redisCommand;
        }

        public async Task StartAsync()
        {
            await _blockCatchup.CatchupAsync();
            await _addressCatchup.CatchupAsync();

            await _redisCommand.SetAsync(Settings.Redis.NodeVersion, (await _nexusQuery.GetInfoAsync()).Version);
            
            await _blockCacheJob.CreateAsync();
            await _blockCache.GetBlocksAsync();

            BackgroundJob.Schedule<JobsInit>(x => x.Start(), TimeSpan.FromSeconds(10));
        }
    }

    public static class HangfireExtensions
    {
        public static void PurgeJobs(this IMonitoringApi monitor)
        {
            //RecurringJobs
            JobStorage.Current.GetConnection().GetRecurringJobs().ForEach(xx => BackgroundJob.Delete(xx.Id));

            //ProcessingJobs
            monitor.ProcessingJobs(0, int.MaxValue).ForEach(xx => BackgroundJob.Delete(xx.Key));

            //ScheduledJobs
            monitor.ScheduledJobs(0, int.MaxValue).ForEach(xx => BackgroundJob.Delete(xx.Key));

            //EnqueuedJobs
            monitor.Queues().ToList().ForEach(xx => monitor.EnqueuedJobs(xx.Name, 0, int.MaxValue).ForEach(x => BackgroundJob.Delete(x.Key)));
        }
    }
}


