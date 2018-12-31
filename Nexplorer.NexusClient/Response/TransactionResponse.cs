using System.Collections.Generic;
using Newtonsoft.Json;
using Nexplorer.NexusClient.JsonConverters;

namespace Nexplorer.NexusClient.Response
{
    public class TransactionResponse
    {
        [JsonProperty("confirmations")]
        public int Confirmations { get; set; }

        [JsonProperty("blockhash")]
        public string BlockHash { get; set; }

        [JsonProperty("txid")]
        public string TransactionHash { get; set; }

        [JsonProperty("time")]
        public int Time { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }
        
        [JsonProperty("outputs")]
        [JsonConverter(typeof(InputOutputConverter))]
        public List<TransactionInputOutputResponse> Outputs { get; set; }

        [JsonProperty("inputs")]
        [JsonConverter(typeof(InputOutputConverter))]
        public List<TransactionInputOutputResponse> Inputs { get; set; }
    }
}