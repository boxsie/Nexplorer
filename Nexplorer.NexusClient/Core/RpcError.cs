using Newtonsoft.Json;

namespace Nexplorer.NexusClient.Core
{
    public class RpcError
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}