using Newtonsoft.Json;

namespace Nexplorer.NexusClient.Response
{
    public class DifficultyResponse
    {
        [JsonProperty("prime")] 
        public double PrimeChannel { get; set; }

        [JsonProperty("hash")]
        public double HashChannel { get; set; }

        [JsonProperty("stake")]
        public double ProofOfStake { get; set; }
    }
}