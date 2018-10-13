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
            var config = new ConfigurationBuilder()
                .AddJsonFile("connectionStrings.json", false)
                .Build();

            var builder = new DbContextOptionsBuilder<NexplorerDb>();

            var connectionString = config.GetConnectionString("NexusDb");

            builder.UseSqlServer(connectionString, x => x.MigrationsAssembly("Nexplorer.Data"));

            return new NexplorerDb(builder.Options);
        }
    }
}