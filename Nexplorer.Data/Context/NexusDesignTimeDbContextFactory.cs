using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Nexplorer.Config;

namespace Nexplorer.Data.Context
{
    public class NexusDesignTimeDbContextFactory : IDesignTimeDbContextFactory<NexusDb>
    {
        public NexusDb CreateDbContext(string[] args)
        {

#if DEBUG
            const string appSettingsFile = "config.debug.json";
#else
            const string appSettingsFile = "config.json";
#endif

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + @"\bin\Debug\netcoreapp2.0\")
                .AddJsonFile(appSettingsFile)
                .Build();

            var builder = new DbContextOptionsBuilder<NexusDb>();

            var connectionString = configuration.GetConnectionString("NexusDb");

            builder.UseMySql(connectionString, x => x.MigrationsAssembly("Nexplorer.Data"));

            return new NexusDb(builder.Options);
        }
    }
}

