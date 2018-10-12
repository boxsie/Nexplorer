using Newtonsoft.Json;

namespace Nexplorer.Client.Response
{
    public class InfoResponse
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("protocolversion")]
        public int ProtocolVersion { get; set; }

        [JsonProperty("walletversion")]
        public int WalletVersion { get; set; }
        
        [JsonProperty("balance")]
        public double Balance { get; set; }

        [JsonProperty("newmint")]
        public double NewMint { get; set; }

        [JsonProperty("stake")]
        public double Stake { get; set; }

        [JsonProperty("interestweight")]
        public double InterestWeight { get; set; }

        [JsonProperty("stakeweight")]
        public double StakeWeight { get; set; }

        [JsonProperty("trustweight")]
        public double TrustWeight { get; set; }
        
        [JsonProperty("blockweight")]
        public double BlockHeight { get; set; }

        [JsonProperty("blocks")]
        public int Blocks { get; set; }

        [JsonProperty("timestamp")]
        public int TimeStamp { get; set; }

        [JsonProperty("connections")]
        public int Connections { get; set; }

        [JsonProperty("proxy")]
        public string Proxy { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("testnet")]
        public bool TestNet { get; set; }

        [JsonProperty("keypoololdest")]
        public int KeyPoolOldest { get; set; }

        [JsonProperty("keypoolsize")]
        public int KeyPoolSize { get; set; }

        [JsonProperty("paytxfee")]
        public double PayTaxFree { get; set; }

        [JsonProperty("errors")]
        public string[] Errors { get; set; }
    }
}
