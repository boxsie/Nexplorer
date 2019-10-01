using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexplorer.Core;
using Nexplorer.Node.Hubs;
using Nexplorer.Node.Services;
using Nexplorer.Node.Settings;

namespace Nexplorer.Node
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
            services.AddSingleton(new Globals());
            var appSettings = Configuration.Get<AppSettings>();
            services.AddSingleton(appSettings);

            services.AddControllers();
            services.AddLogging(logging => { logging.AddConsole(); })
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

            services.AddNexusServices(appSettings.NodeEndpoint);

            services.AddHostedService<NewBlockService>();

            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NexplorerHub>("/nexplorer");
            });
        }
    }
}
