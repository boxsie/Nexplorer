using Nexplorer.Domain.Enums;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class AddressDistrubtionBandDto
    {
        [ProtoMember(1)]
        public AddressBalanceDistributionBands DistributionBand { get; set; }
        
        [ProtoMember(2)]
        public int AddressCount { get; set; }

        [ProtoMember(3)]
        public double AddressPercent { get; set; }

        [ProtoMember(4)]
        public double CoinBalance { get; set; }

        [ProtoMember(5)]
        public double CoinPercent { get; set; }
    }
}