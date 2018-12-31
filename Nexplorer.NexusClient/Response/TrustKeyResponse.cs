using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nexplorer.NexusClient.Response
{
    public class TrustKeyResponse
    {
        [JsonProperty("keys")]
        public List<TrustKeyAddressResponse> Keys { get; set; }

        [JsonProperty("total active")]
        public int TotalActive { get; set; }

        [JsonProperty("total expired")]
        public int TotalExpired { get; set; }
    }
}