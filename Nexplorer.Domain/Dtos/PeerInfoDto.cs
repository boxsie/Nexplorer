using System;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class PeerInfoDto
    {
        [ProtoMember(1)]
        public string Address { get; set; }

        [ProtoMember(2)]
        public string Services { get; set; }

        [ProtoMember(3)]
        public DateTime LastSendTime { get; set; }

        [ProtoMember(4)]
        public DateTime LastReceiveTime { get; set; }

        [ProtoMember(5)]
        public DateTime ConnectionTime { get; set; }

        [ProtoMember(6)]
        public int Version { get; set; }

        [ProtoMember(7)]
        public string VersionInfo { get; set; }

        [ProtoMember(8)]
        public int ChainHeight { get; set; }

        [ProtoMember(9)]
        public int Banscore { get; set; }

        [ProtoMember(10)]
        public GeolocationDto Geolocation { get; set; }
    }
}