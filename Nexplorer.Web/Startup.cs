using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Nexplorer.Config;
using Nexplorer.Config.Core;
using Nexplorer.Core;
using Nexplorer.Data.Command;
using Nexplorer.Data.Context;
using Nexplorer.Data.Map;
using Nexplorer.Data.Query;
using Nexplorer.Data.Services;
using Nexplorer.Domain.Entity.User;
using Nexplorer.Infrastructure.Bittrex;
using Nexplorer.Infrastructure.Currency;
using Nexplorer.Infrastructure.Geolocate;
using Nexplorer.NexusClient;
using Nexplorer.NexusClient.Core;
using Nexplorer.Web;
using Nexplorer.Web.Auth;
using Nexplorer.Web.Enums;
using Nexplorer.Web.Extensions;
using Nexplorer.Web.Hubs;
using Nexplorer.Web.Queries;
using Nexplorer.Web.Services.Email;
using Nexplorer.Web.Services.User;
using NLog.Extensions.Logging;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Swagger;

namespace Nexplorer.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddCors();

            var config = Settings.BuildConfig(services);

            services.AddDbContext<NexusDb>(x => x.UseSqlServer(config.GetConnectionString("NexusDb"), y => y.MigrationsAssembly("Nexplorer.Data")), ServiceLifetime.Transient);
            services.AddDbContext<NexplorerDb>(x => x.UseSqlServer(config.GetConnectionString("NexplorerDb"), y => y.MigrationsAssembly("Nexplorer.Data")), ServiceLifetime.Transient);

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddEntityFrameworkStores<NexplorerDb>()
                .AddDefaultTokenProviders();

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;
                
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;
                
                options.User.RequireUniqueEmail = true;
            });

            services.AddAuthorization(options =>
            {
                var roles = Enum.GetNames(typeof(UserRoles));

                options.AddPolicy(UserConfig.SuperUserPolicy, policy => policy.RequireRole(roles.Take(1)));
                options.AddPolicy(UserConfig.AdminUserPolicy, policy => policy.RequireRole(roles.Take(2)));
                options.AddPolicy(UserConfig.EditorUserPolicy, policy => policy.RequireRole(roles.Take(3)));
                options.AddPolicy(UserConfig.UserPolicy, policy => policy.RequireRole(roles));
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Nexplorer API", Version = "v1" });
                c.DescribeAllParametersInCamelCase();
                c.DescribeAllEnumsAsStrings();
                c.DescribeStringEnumsInCamelCase();
            });

            services.AddSingleton(ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")));
            services.AddSingleton<RedisCommand>();

            services.AddSingleton<AutoMapperConfig>();
            services.AddSingleton(x => x.GetService<AutoMapperConfig>().GetMapper());
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<AddressFavouriteCommand>();

            services.AddTransient<BlockPublishCommand>();
            services.AddTransient<BlockCacheCommand>();
            services.AddScoped<UserService>();

            services.AddTransient<INxsClient, NxsClient>();
            services.AddTransient<INxsClient, NxsClient>(x => new NxsClient(config.GetConnectionString("Nexus")));

            services.AddTransient<BlockQuery>();
            services.AddTransient<TransactionQuery>();
            services.AddTransient<AddressQuery>();
            services.AddTransient<ExchangeQuery>();
            services.AddTransient<UserQuery>();
            services.AddTransient<CurrencyQuery>();
            services.AddTransient<StatQuery>();
            services.AddTransient<NexusQuery>();

            services.AddSingleton<CurrencyClient>();

            services.AddScoped<LayoutHub>();
            services.AddSingleton<LayoutMessenger>();
            services.AddScoped<HomeHub>();
            services.AddSingleton<HomeMessenger>();
            services.AddScoped<BlockHub>();
            services.AddSingleton<BlockMessenger>();
            services.AddScoped<TransactionHub>();
            services.AddSingleton<TransactionMessenger>();
            services.AddScoped<MiningHub>();
            services.AddSingleton<MiningMessenger>();
            services.AddScoped<AddressHub>();
            services.AddSingleton<AddressMessenger>();
            services.AddScoped<AdminHub>();
            services.AddSingleton<AdminMessenger>();

#if DEBUG
            services.AddSignalR(o => { o.EnableDetailedErrors = true; });
#else
            services.AddSignalR();
#endif
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            Settings.AttachConfig(serviceProvider);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
                app.UseExceptionHandler("/Home/Error");

            loggerFactory.AddNLog();

#if DEBUG
            app.UseStaticFiles();
#else
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={60 * 60 * 24 * 7}";
                }
            });
#endif
            app.UseAuthentication();
            serviceProvider.GetService<UserService>().CreateRoles().GetAwaiter().GetResult();

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-GB")
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nexplorer API V1");
            });
            
            app.UseMvc(routes =>
            {
                routes.MapRoute("home", "{controller=home}/{action=index}");
                routes.MapRoute("account", "{controller=account}/{action=index}");
                routes.MapRoute("manage", "{controller=manage}/{action=index}");
                routes.MapRoute("blocks", "blocks/{blockId}", new { controller = "blocks", action = "block" });
                routes.MapRoute("transactions", "transactions/{txHash}", new { controller = "transactions", action = "transaction" });
                routes.MapRoute("addresses", "addresses/{addressHash}", new { controller = "addresses", action = "address" });
                routes.MapRoute("cookie", "cookie", new { controller = "home", action = "cookie" });
            });
            
            app.UseSignalR(routes =>
            {
                routes.MapHub<LayoutHub>("/layouthub");
                routes.MapHub<HomeHub>("/homehub");
                routes.MapHub<BlockHub>("/blockhub");
                routes.MapHub<TransactionHub>("/transactionhub");
                routes.MapHub<MiningHub>("/mininghub");
                routes.MapHub<AddressHub>("/addresshub");
                routes.MapHub<AdminHub>("/adminhub");
            });

            serviceProvider.GetService<HomeMessenger>();
            serviceProvider.GetService<LayoutMessenger>();
            serviceProvider.GetService<BlockMessenger>();
            serviceProvider.GetService<TransactionMessenger>();
            serviceProvider.GetService<MiningMessenger>();
            serviceProvider.GetService<AddressMessenger>();
            serviceProvider.GetService<AdminMessenger>();

            // Migrate EF
            serviceProvider.GetService<NexplorerDb>().Database.Migrate();
        }
    }
}
