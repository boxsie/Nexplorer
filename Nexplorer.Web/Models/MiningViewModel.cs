using System.Collections.Generic;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Web.Models
{
    public class MiningViewModel
    {
        public Dictionary<string, List<ChannelStatDto>> ChannelStats { get; set; }
        public int ChartDurationMs { get; set; }
        public SupplyRateDto SuppyRates { get; set; }
    }
}