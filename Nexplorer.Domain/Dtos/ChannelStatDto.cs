using System;
using Nexplorer.Domain.Enums;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class ChannelStatDto
    {
        [ProtoMember(1)]
        public string Channel { get; set; }

        [ProtoMember(2)]
        public double Difficulty { get; set; }

        [ProtoMember(3)]
        public int Height { get; set; }

        [ProtoMember(4)]
        public double Reward { get; set; }

        [ProtoMember(5)]
        public long RatePerSecond { get; set; }

        [ProtoMember(6)]
        public double Reserve { get; set; }

        [ProtoMember(7)]
        public DateTime CreatedOn { get; set; }
    }
}