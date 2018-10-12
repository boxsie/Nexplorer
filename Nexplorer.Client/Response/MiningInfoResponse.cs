using Newtonsoft.Json;

namespace Nexplorer.Client.Response
{
    public class MiningInfoResponse
    {
        [JsonProperty("blocks")]
        public int Blocks { get; set; }

        [JsonProperty("timestamp")]
        public int TimeStampUtc { get; set; }

        [JsonProperty("currentblocksize")]
        public int CurrentBlockSize { get; set; }

        [JsonProperty("currentblocktx")]
        public int CurrentBlockTx { get; set; }

        [JsonProperty("primeDifficulty")]
        public double PrimeDifficulty { get; set; }

        [JsonProperty("hashDifficulty")]
        public double HashDifficulty { get; set; }

        [JsonProperty("primeReserve")]
        public double PrimeReserve { get; set; }

        [JsonProperty("hashReserve")]
        public double HashReserve { get; set; }

        [JsonProperty("primeValue")]
        public double PrimeValue { get; set; }

        [JsonProperty("hashValue")]
        public double HashValue { get; set; }

        [JsonProperty("pooledtx")]
        public int PooledTx { get; set; }

        [JsonProperty("primesPerSecond")]
        public long PrimesPerSecond { get; set; }

        [JsonProperty("hashPerSecond")]
        public long HashPerSecond { get; set; }
    }
}