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
            var config = new ConfigurationBuilder()
                .AddJsonFile("connectionStrings.json", false)
                .Build();

            var builder = new DbContextOptionsBuilder<NexusDb>();

            var connectionString = config.GetConnectionString("NexusDb");

            builder.UseSqlServer(connectionString, x => x.MigrationsAssembly("Nexplorer.Data"));

            return new NexusDb(builder.Options);
        }
    }
}

