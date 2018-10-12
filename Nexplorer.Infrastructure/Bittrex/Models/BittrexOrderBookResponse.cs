using Newtonsoft.Json;

namespace Nexplorer.Infrastructure.Bittrex.Models
{
    public class BittrexOrderBookResponse
    {
        [JsonProperty("buy")]
        public BittrexBuy[] Buy { get; set; }

        [JsonProperty("sell")]
        public BittrexSell[] Sell { get; set; }
    }
}