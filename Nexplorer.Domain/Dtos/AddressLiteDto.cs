using System;
using System.Text;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class AddressLiteDto
    {
        [ProtoMember(1)]
        public int AddressId { get; set; }

        [ProtoMember(2)]
        public string Hash { get; set; }

        [ProtoMember(3)]
        public double Balance { get; set; }

        [ProtoMember(4)]
        public int FirstBlockSeen { get; set; }

        [ProtoMember(5)]
        public int LastBlockSeen { get; set; }

        [ProtoMember(6)]
        public double? InterestRate { get; set; }
        
        [ProtoMember(7)]
        public bool IsNexus { get; set; }
    }
}
