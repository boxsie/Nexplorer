using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class GeolocationDto
    {
        [ProtoMember(1)]
        public string CountryCode { get; set; }

        [ProtoMember(2)]
        public string CountryName { get; set; }

        [ProtoMember(3)]
        public string RegionCode { get; set; }

        [ProtoMember(4)]
        public string RegionName { get; set; }

        [ProtoMember(5)]
        public string City { get; set; }

        [ProtoMember(6)]
        public string ZipCode { get; set; }

        [ProtoMember(7)]
        public string TimeZone { get; set; }

        [ProtoMember(8)]
        public float Latitude { get; set; }

        [ProtoMember(9)]
        public float Longitude { get; set; }
    }
}