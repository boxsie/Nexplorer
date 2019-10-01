using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Nexplorer.Nexus.Ledger.Models;

namespace Nexplorer.Data
{
    public static class StartupExtentions
    {
        public static void AddDataServices(this IServiceCollection services, NexusDbSettings dbSettings)
        {
            services.AddSingleton(dbSettings);
            services.AddSingleton<IMongoClient>(x => new MongoClient(dbSettings.ConnectionString));
            services.AddSingleton<IBlockDb, BlockDb>();

            BsonClassMap.RegisterClassMap<Block>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(m => m.Height);
            });
        }
    }
}