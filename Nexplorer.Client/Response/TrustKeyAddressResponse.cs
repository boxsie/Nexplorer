using Newtonsoft.Json;
using Nexplorer.Client.JsonConverters;

namespace Nexplorer.Client.Response
{
    public class TrustKeyAddressResponse
    {
        [JsonProperty("address")]
        public string AddressHash { get; set; }

        [JsonProperty("interest rate")]
        public double InterestRate { get; set; }

        [JsonProperty("trust key")]
        [JsonConverter(typeof(TrustKeyDataConverter))]
        public TrustKeyDataResponse Key { get; set; }
    }
}