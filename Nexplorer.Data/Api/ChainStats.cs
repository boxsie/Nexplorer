using System.Collections.Generic;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Data.Api
{
    public class ChainStats
    {
        public int TotalHeight { get; set; }
        public double TotalSupply { get; set; }
        public List<ChannelStatDto> Channels { get; set; }
    }
}
