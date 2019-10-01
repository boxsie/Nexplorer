using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nexplorer.Data
{
    public class NexusDbSettings
    {
        public string BlockCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
