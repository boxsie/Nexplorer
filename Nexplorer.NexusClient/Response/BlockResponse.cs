using System;
using Newtonsoft.Json;
using Nexplorer.NexusClient.JsonConverters;

namespace Nexplorer.NexusClient.Response
{
    public class BlockResponse
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("channel")]
        public int Channel { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("merkleroot")]
        public string MerkleRoot { get; set; }

        [JsonProperty("time")]
        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime Timestamp { get; set; }

        [JsonProperty("nonce")]
        public double Nonce { get; set; }

        [JsonProperty("bits")]
        public string Bits { get; set; }

        [JsonProperty("difficulty")]
        public double Difficulty { get; set; }

        [JsonProperty("mint")]
        public double Mint { get; set; }

        [JsonProperty("previousblockhash")]
        public string PreviousBlockHash { get; set; }

        [JsonProperty("nextblockhash")]
        public string NextBlockHash { get; set; }

        [JsonProperty("tx")]
        public string[] TransactionHash { get; set; }
    }
}