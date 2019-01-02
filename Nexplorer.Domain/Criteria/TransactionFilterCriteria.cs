using System;
using Newtonsoft.Json;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Domain.Criteria
{
    public class BlockFilterCriteria
    {
        [JsonProperty("heightFrom")]
        public int? HeightFrom { get; set; }

        [JsonProperty("heightTo")]
        public int? HeightTo { get; set; }

        [JsonProperty("minSize")]
        public double? MinSize { get; set; }

        [JsonProperty("maxSize")]
        public double? MaxSize { get; set; }

        [JsonProperty("utcFrom")]
        public DateTime? UtcFrom { get; set; }

        [JsonProperty("utcTo")]
        public DateTime? UtcTo { get; set; }

        [JsonProperty("orderBy")]
        public OrderBlocksBy OrderBy { get; set; }
    }

    public class TransactionFilterCriteria
    {
        [JsonProperty("minAmount")]
        public double? MinAmount { get; set; }

        [JsonProperty("maxAmount")]
        public double? MaxAmount { get; set; }

        [JsonProperty("heightFrom")]
        public int? HeightFrom { get; set; }

        [JsonProperty("heightTo")]
        public int? HeightTo { get; set; }

        [JsonProperty("utcFrom")]
        public DateTime? UtcFrom { get; set; }

        [JsonProperty("utcTo")]
        public DateTime? UtcTo { get; set; }

        [JsonProperty("isStakeReward")]
        public bool? IsStakeReward { get; set; }
        
        [JsonProperty("isMiningReward")]
        public bool? IsMiningReward { get; set; }

        [JsonProperty("addressHashes")]
        public string[] AddressHashes { get; set; }

        [JsonProperty("orderBy")]
        public OrderTransactionsBy OrderBy { get; set; }
    }
}