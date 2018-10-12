using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class AddressStatDto
    {
        [ProtoMember(1)]
        public long AddressCount { get; set; }

        [ProtoMember(2)]
        public double AverageBalance { get; set; }

        [ProtoMember(3)]
        public double CreatedPerHour { get; set; }

        [ProtoMember(4)]
        public int StakingCount { get; set; }

        [ProtoMember(5)]
        public int BalanceOverOneThousand { get; set; }

        [ProtoMember(6)]
        public int DormantOverOneYear { get; set; }

        [ProtoMember(7)]
        public int ZeroBalance { get; set; }
    }
}