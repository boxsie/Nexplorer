using Newtonsoft.Json;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Domain.Criteria
{
    public class AddressFilterCriteria
    {
        [JsonProperty("minBalance")]
        public double? MinBalance { get; set; }

        [JsonProperty("maxBalance")]
        public double? MaxBalance { get; set; }

        [JsonProperty("heightFrom")]
        public int? HeightFrom { get; set; }

        [JsonProperty("heightTo")]
        public int? HeightTo { get; set; }

        [JsonProperty("isStaking")]
        public bool IsStaking { get; set; }

        [JsonProperty("isNexus")]
        public bool IsNexus { get; set; }

        [JsonProperty("orderBy")]
        public OrderAddressesBy OrderBy { get; set; }
    }
}