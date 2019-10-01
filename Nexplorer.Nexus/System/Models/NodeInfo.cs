namespace Nexplorer.Nexus.System.Models
{
    public class NodeInfo
    {
        public string Version { get; set; }
        public int Protocolversion { get; set; }
        public int Walletversion { get; set; }
        public long Timestamp { get; set; }
        public int Testnet { get; set; }
        public bool Private { get; set; }
        public bool Multiuser { get; set; }
        public int Blocks { get; set; }
        public bool Synchronizing { get; set; }
        public int Synccomplete { get; set; }
        public int Txtotal { get; set; }
        public int Connections { get; set; }
        public string[] Eids { get; set; }
    }
}