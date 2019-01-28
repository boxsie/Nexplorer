namespace Nexplorer.Config.Core
{
    public class ConnectionStrings
    {
        public bool UseTest { get; set; }
        public string Redis { get; set; }
        public string NexusDb { get; set; }
        public string NexplorerDb { get; set; }
        public string NexplorerTest { get; set; }
        public string Nexus { get; set; }

        public string GetNexusDbConnectionString()
        {
            return UseTest ? NexplorerTest : NexusDb;
        }

        public string GetNexplorerDbConnectionString()
        {
            return UseTest ? NexplorerTest : NexplorerDb;
        }
    }
}