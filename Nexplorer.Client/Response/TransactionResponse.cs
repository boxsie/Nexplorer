using Newtonsoft.Json;

namespace Nexplorer.Client.Response
{
    public class TransactionResponse
    {
        [JsonProperty("confirmations")]
        public int Confirmations { get; set; }

        [JsonProperty("blockhash")]
        public string BlockHash { get; set; }

        [JsonProperty("txid")]
        public string TxId { get; set; }

        [JsonProperty("time")]
        public int Time { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("inputs")]
        public string[] Inputs { get; set; }

        [JsonProperty("outputs")]
        public string[] Outputs { get; set; }
    }
}