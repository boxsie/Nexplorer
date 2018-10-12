using System;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class TrustKeyDto
    {
        [ProtoMember(1)]
        public string TrustKey { get; set; }

        [ProtoMember(2)]
        public string TrustHash { get; set; }

        [ProtoMember(3)]
        public int AddressId { get; set; }

        [ProtoMember(4)]
        public string AddressHash { get; set; }

        [ProtoMember(5)]
        public int TransactionId { get; set; }

        [ProtoMember(6)]
        public string TransactionHash { get; set; }

        [ProtoMember(7)]
        public int GenesisBlockHeight { get; set; }

        [ProtoMember(8)]
        public double InterestRate { get; set; }

        [ProtoMember(9)]
        public DateTime TimeUtc { get; set; }

        [ProtoMember(10)]
        public TimeSpan TimeSinceLastBlock { get; set; }
    }
}