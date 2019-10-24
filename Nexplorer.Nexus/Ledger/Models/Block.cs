using System.Collections.Generic;
using System.Numerics;

namespace Nexplorer.Nexus.Ledger.Models
{
    public class Block
    {
        public string Hash { get; set; }
        public string ProofHash { get; set; }
        public int Size { get; set; }
        public int Height { get; set; }
        public int Channel { get; set; }
        public int Version { get; set; }
        public string MerkleRoot { get; set; }
        public string Time { get; set; }
        public string Nonce { get; set; }
        public string Bits { get; set; }
        public double Difficulty { get; set; }
        public double Mint { get; set; }
        public string PreviousBlockHash { get; set; }
        public string NextBlockHash { get; set; }
        public List<Transaction> Tx { get; set; }
    }
}