using Newtonsoft.Json;

namespace Nexplorer.Client.Response
{
    public class DifficultyResponse
    {
        [JsonProperty("prime - channel")] 
        public double PrimeChannel { get; set; }

        [JsonProperty("hash  - channel")]
        public double HashChannel { get; set; }

        [JsonProperty("proof-of-stake")]
        public double ProofOfStake { get; set; }
    }
}