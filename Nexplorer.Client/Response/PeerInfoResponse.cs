using Newtonsoft.Json;

namespace Nexplorer.Client.Response
{
    public class PeerInfoResponse
    {
        [JsonProperty("addr")]
        public string IpAddress { get; set; }

        [JsonProperty("services")]
        public string Services { get; set; }

        [JsonProperty("lastsend")]
        public int LastSendTime { get; set; }

        [JsonProperty("lastrecv")]
        public int LastReceiveTime { get; set; }
        
        [JsonProperty("conntime")]
        public int ConnectionTime { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("subver")]
        public string SubVersion { get; set; }

        [JsonProperty("inbound")]
        public bool Inbound { get; set; }

        [JsonProperty("releasetime")]
        public int ReleaseTime { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("banscore")]
        public int Banscore { get; set; }
    }
}