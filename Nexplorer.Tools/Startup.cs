

using System;
using System.Linq;
using System.Threading.Tasks;
using Boxsie.DotNetNexusClient;
using Boxsie.DotNetNexusClient.Core;
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

            services.AddScoped<INexusClient, NexusClient>();
            services.AddScoped<INexusClient, NexusClient>(x => new NexusClient(config.GetConnectionString("Nexus")));
            services.AddScoped<NexusBlockStream>();

            services.AddTransient<App>();

            services.AddTransient<BlockSyncCatchup>();
            services.AddTransient<AddressAggregateCatchup>();
            services.AddTransient<BlockRewardCatchup>();
            services.AddScoped<BlockCacheJob>();
            services.AddScoped<BlockPublishJob>();
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
        private readonly NexusQuery _query;
        private readonly BlockCacheJob _cacheJob;
        private readonly NexusBlockStream _stream;
        private readonly BlockSyncCatchup _blockCatchup;
        private readonly AddressAggregateCatchup _addressCatchup;

        public App(NexusQuery query, BlockCacheJob cacheJob, NexusBlockStream stream, BlockSyncCatchup blockCatchup, AddressAggregateCatchup addressCatchup)
        {
            _query = query;
            _cacheJob = cacheJob;
            _stream = stream;
            _blockCatchup = blockCatchup;
            _addressCatchup = addressCatchup;
        }

        public async Task StartAsync()
        {
            await _blockCatchup.CatchupAsync();
            await _addressCatchup.CatchupAsync();

            await StartJobsAsync();
        }

        private async Task StartJobsAsync()
        {
            BackgroundJob.Enqueue<BlockCacheJob>(x => x.CreateAsync());
            BackgroundJob.Schedule<BlockSyncJob>(x => x.SyncLatestAsync(), TimeSpan.FromMinutes(1));

            await _stream.Start(TimeSpan.FromSeconds(3));

            _stream.Subscribe(async blockResponse =>
            {
                var blockDto = await _query.MapResponseToDtoAsync(blockResponse, true);

                BackgroundJob.Enqueue<BlockCacheJob>(x => x.AddAsync(blockDto.Height, true));
            });
        }
    }

    public static class HangfireExtensions
    {
        public static void PurgeJobs(this IMonitoringApi monitor)
        {
            var jobIds = monitor.ScheduledJobs(0, int.MaxValue)
                .Select(x => x.Key)
                .Concat(monitor.ProcessingJobs(0, int.MaxValue)
                    .Select(x => x.Key))
                .ToList();

            foreach (var jobId in jobIds)
                BackgroundJob.Delete(jobId);
        }
    }
}


