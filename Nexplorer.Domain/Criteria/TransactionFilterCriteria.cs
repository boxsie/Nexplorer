using System;
using Newtonsoft.Json;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Domain.Criteria
{
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
        public bool IsStakeReward { get; set; }
        
        [JsonProperty("isMiningReward")]
        public bool IsBlockReward { get; set; }

        [JsonProperty("fromAddressHashes")]
        public string[] FromAddressHashes { get; set; }

        [JsonProperty("toAddressHashes")]
        public string[] ToAddressHashes { get; set; }

        [JsonProperty("fromAddress")]
        public OrderTransactionsBy OrderBy { get; set; }
    }
}