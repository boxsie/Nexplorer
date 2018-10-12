using Newtonsoft.Json;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Data.Api
{
    public abstract class FilterCriteria
    {
        /// <summary>
        /// The start index of the filtered results to return.
        /// </summary>
        [JsonProperty("startAt")]
        public int? StartAt { get; set; }

        /// <summary>
        /// The amount of filtered addresses to return, maximum 250.
        /// </summary>
        [JsonProperty("count")]
        public int? Count { get; set; }
    }


    /// <summary>
    /// The address filter criteria
    /// </summary>
    public class AddressesCriteria : FilterCriteria
    {
        /// <summary>
        /// The inclusive balance to filter addresses from.
        /// </summary>
        [JsonProperty("balanceFrom")]
        public double? BalanceFrom { get; set; }

        /// <summary>
        /// The inclusive balance to filter addresses to.
        /// </summary>
        [JsonProperty("balanceTo")]
        public double? BalanceTo { get; set; }

        /// <summary>
        /// The inclusive minimum block height this addresses was seen in.
        /// </summary>
        [JsonProperty("lastBockHeightFrom")]
        public int? LastBockHeightFrom { get; set; }

        /// <summary>
        /// The inclusive maximum block height this addresses was seen in.
        /// </summary>
        [JsonProperty("lastBlockHeightTo")]
        public int? LastBlockHeightTo { get; set; }

        /// <summary>
        /// Only return addresses that are staking.
        /// </summary>
        [JsonProperty("isStaking")]
        public bool? IsStaking { get; set; }

        /// <summary>
        /// Only return addresses that are controlled by Nexus Earth.
        /// </summary>
        [JsonProperty("isNexus")]
        public bool? IsNexus { get; set; }

        /// <summary>
        /// How the filtered addresses should be ordered.
        /// 
        /// 0 - Highest balance
        /// 1 - Lowest balance
        /// 2 = Most recently seen
        /// 3 = Least recently seen
        /// 4 = Highest interest rate (only works if 'IsStaking' is set to true)
        /// 5 = Lowest Interest Rate (only works if 'IsStaking' is set to true)
        /// </summary>
        [JsonProperty("orderBy")]
        public OrderAddressesBy OrderBy { get; set; }
    }
}
