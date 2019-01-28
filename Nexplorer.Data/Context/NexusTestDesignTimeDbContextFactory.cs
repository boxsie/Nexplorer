using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Nexplorer.Data.Context
{
    public class NexusTestDesignTimeDbContextFactory : IDesignTimeDbContextFactory<NexusTestDb>
    {
        public NexusTestDb CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("connectionStrings.json", false)
                .Build();

            var builder = new DbContextOptionsBuilder<NexusDb>();

            var connectionString = config.GetConnectionString("NexplorerTest");

            builder.UseSqlServer(connectionString, x =>
            {
                x.MigrationsAssembly("Nexplorer.Data");
                x.CommandTimeout((int)TimeSpan.FromHours(2).TotalSeconds);
            });

            return new NexusTestDb(builder.Options);
        }
    }
}