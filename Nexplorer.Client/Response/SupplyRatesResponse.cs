using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Nexplorer.Client.Response
{
    public class SupplyRatesResponse
    {
        [JsonProperty("chainAge")]
        public int ChainAge { get; set; }

        [JsonProperty("moneysupply")]
        public double MoneySupply { get; set; }

        [JsonProperty("targetsupply")]
        public double TargetSupply { get; set; }

        [JsonProperty("inflationrate")]
        public double InflationRate { get; set; }

        [JsonProperty("minuteSupply")]
        public double MinuteSupply { get; set; }

        [JsonProperty("hourSupply")]
        public double HourSupply { get; set; }

        [JsonProperty("daySupply")]
        public double DaySupply { get; set; }

        [JsonProperty("weekSupply")]
        public double WeekSupply { get; set; }

        [JsonProperty("monthSupply")]
        public double MonthSupply { get; set; }

        [JsonProperty("yearSupply")]
        public double YearSupply { get; set; }
    }
}
