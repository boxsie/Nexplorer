using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Nexplorer.Data.Context
{
    public class NexplorerDesignTimeDbContextFactory : IDesignTimeDbContextFactory<NexplorerDb>
    {
        public NexplorerDb CreateDbContext(string[] args)
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

            var builder = new DbContextOptionsBuilder<NexplorerDb>();

            var connectionString = configuration.GetConnectionString("NexplorerDb");

            builder.UseMySql(connectionString, x => x.MigrationsAssembly("Nexplorer.Data"));

            return new NexplorerDb(builder.Options);
        }
    }
}