using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexplorer.Config;
using Nexplorer.Data.Command;
using Nexplorer.Data.Context;
using Nexplorer.Data.Map;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.NexusClient;

namespace Nexplorer.Tests
{
    public class BlockSyncFixture : IDisposable
    {
        public NxsClient Client { get; private set; }
        public BlockInsertCommand InsertCommand { get; private set; }
        public BlockDeleteCommand DeleteCommand { get; private set; }
        public AddressAggregatorCommand AddressAggregator { get; private set; }
        public NexusQuery NexusQuery { get; private set; }
        public BlockQuery BlockQuery { get; private set; }
        public AddressQuery AddressQuery { get; private set; }
        public IMapper Mapper { get; private set; }

        public BlockSyncFixture()
        {
            var sc = new ServiceCollection();

            Settings.BuildConfig(sc);
            Settings.AttachConfig(sc.BuildServiceProvider(), true);

            var testDb = new NexusTestDesignTimeDbContextFactory().CreateDbContext(null);

            Client = new NxsClient(Settings.Connection.Nexus);
            InsertCommand = new BlockInsertCommand();
            DeleteCommand = new BlockDeleteCommand();
            AddressAggregator = new AddressAggregatorCommand();
            Mapper = new AutoMapperConfig().GetMapper();
            NexusQuery = new NexusQuery(Client, Mapper);
            BlockQuery = new BlockQuery(testDb, Mapper);
            AddressQuery = new AddressQuery(testDb, null);
        }

        public void Dispose()
        {

        }
    }
}