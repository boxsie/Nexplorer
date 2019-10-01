namespace Nexplorer.Nexus.System.Models
{
    public class Peer
    {
        public string Address { get; set; }
        public string Version { get; set; }
        public int Height { get; set; }
        public string Latency { get; set; }
        public long Lastseen { get; set; }
        public int Connects { get; set; }
        public int Drops { get; set; }
        public int Fails { get; set; }
        public double Score { get; set; }
    }
}