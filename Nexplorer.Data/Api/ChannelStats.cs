using System.Collections.Generic;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Data.Api
{
    public class ChannelStats
    {
        public int TotalHeight { get; set; }
        public List<ChannelStatDto> Channels { get; set; }
    }
}
