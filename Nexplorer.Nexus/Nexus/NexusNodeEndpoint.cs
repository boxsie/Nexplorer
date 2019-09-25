namespace Nexplorer.Nexus.Nexus
{
    public class NexusNodeEndpoint
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool ApiSessions { get; set; }
        public bool IndexHeight { get; set; }
    }
}