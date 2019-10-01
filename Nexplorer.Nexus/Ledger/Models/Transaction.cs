namespace Nexplorer.Nexus.Ledger.Models
{
    public class Transaction
    {
        public string Hash { get; set; }
        public string Type { get; set; }
        public int Version { get; set; }
        public int Sequence { get; set; }
        public int Timestamp { get; set; }
        public string Genesis { get; set; }
        public int Confirmations { get; set; }
        public string NextHash { get; set; }
        public string PrevHash { get; set; }
        public string PubKey { get; set; }
        public string Signature { get; set; }
        public InputOutput[] Inputs { get; set; }
        public InputOutput[] Outputs { get; set; }
        public Operation Operation { get; set; }
    }
}