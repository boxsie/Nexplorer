namespace Nexplorer.Nexus.Ledger.Models
{
    public class Operation
    {
        public string Op { get; set; }
        public int Nonce { get; set; }
        public int Amount { get; set; }
    }
}