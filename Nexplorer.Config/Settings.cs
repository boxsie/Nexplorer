using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nexplorer.Config.Core;

namespace Nexplorer.Config
{
    public static class Settings
    {
        public static AppConfig App { get; private set; }
        public static ConnectionStrings Connection { get; private set; }
        public static RedisKeys Redis { get; private set; }
        public static EmailConfig EmailConfig { get; private set; }
        public static UserConfig UserConfig { get; private set; }

        public static IConfigurationRoot BuildConfig(IServiceCollection services)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appConfig.json", false)
                .AddJsonFile("connectionStrings.json", false)
                .AddJsonFile("emailConfig.json", false)
                .AddJsonFile("redisKeys.json", false)
                .AddJsonFile("userConfig.json", false)
                .Build();

            services.Configure<AppConfig>(config.GetSection("AppConfig"));
            services.Configure<ConnectionStrings>(config.GetSection("ConnectionStrings"));
            services.Configure<RedisKeys>(config.GetSection("RedisKeys"));
            services.Configure<EmailConfig>(config.GetSection("EmailConfig"));
            services.Configure<UserConfig>(config.GetSection("UserConfig"));

            return config;
        }

        public static void AttachConfig(IServiceProvider provider)
        {
            App = provider.GetRequiredService<IOptions<AppConfig>>().Value;
            Connection = provider.GetRequiredService<IOptions<ConnectionStrings>>().Value;
            Redis = provider.GetRequiredService<IOptions<RedisKeys>>().Value;
            EmailConfig = provider.GetRequiredService<IOptions<EmailConfig>>().Value;
            UserConfig = provider.GetRequiredService<IOptions<UserConfig>>().Value;
        }
    }
}
