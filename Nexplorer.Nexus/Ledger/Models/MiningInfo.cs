namespace Nexplorer.Nexus.Ledger.Models
{
    public class MiningInfo
    {
        public int Blocks { get; set; }
        public int Timestamp { get; set; }
        public double StakeDifficulty { get; set; }
        public double PrimeDifficulty { get; set; }
        public double HashDifficulty { get; set; }
        public double PrimeReserve { get; set; }
        public double HashReserve { get; set; }
        public double PrimeValue { get; set; }
        public double HashValue { get; set; }
        public int PooledTx { get; set; }
        public double PrimesPerSecond { get; set; }
        public double HashPerSecond { get; set; }
        public int TotalConnections { get; set; }
    }
}