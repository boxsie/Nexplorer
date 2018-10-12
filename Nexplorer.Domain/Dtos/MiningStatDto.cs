using System.Collections.Generic;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class MiningStatDto
    {
        [ProtoMember(1)]
        public Dictionary<string, ChannelStatDto> ChannelStats { get; set; }

        [ProtoMember(2)]
        public SupplyRateDto SupplyRate { get; set; }
    }
}