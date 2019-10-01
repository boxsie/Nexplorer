using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nexplorer.Core;

namespace Nexplorer.Node
{
    public static class Program
    {
        public static string CurrentEnvironment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        public static void Main(string[] args)
        {
            BuildWebHost(args, CurrentEnvironment).Run();
        }

        public static IWebHost BuildWebHost(string[] args, string environment)
        {
            var configuration = CreateConfigurationBuilder(args, environment)
                .Build();

            var webHost = CreateWebHostBuilder(configuration)
                .Build();

            return webHost;
        }

        public static IConfigurationBuilder CreateConfigurationBuilder(string[] args, string environment)
        {
            return new ConfigurationBuilder()
                .AddApplicationPath(environment)
                .AddApplicationSettings(environment)
                //.AddSecrets(environment)
                .AddCommandLine(args);
        }

        public static IWebHostBuilder CreateWebHostBuilder(IConfiguration configuration)
        {
            return new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseIISIntegration()
                .UseConfiguration(configuration)
                .UseStartup<Startup>();
        }
    }
}
