using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Nexplorer.Domain.Dtos;
using Nexplorer.Infrastructure.Geolocate;

namespace Nexplorer.Data.Services
{
    public class GeolocationService
    {
        private readonly GeolocateIpClient _geolocateIpClient;
        private readonly IMapper _mapper;

        private readonly Dictionary<string, GeolocationDto> _geolocationCache;

        public GeolocationService(GeolocateIpClient geolocateIpClient, IMapper mapper)
        {
            _geolocateIpClient = geolocateIpClient;
            _mapper = mapper;

            _geolocationCache = new Dictionary<string, GeolocationDto>();
        }

        public async Task<GeolocationDto> GetGeolocation(string ip)
        {
            if (_geolocationCache.ContainsKey(ip))
                return _geolocationCache[ip];

            var geoDto = _mapper.Map<GeolocationDto>(await _geolocateIpClient.GetGeolocationFromIp(ip));

            _geolocationCache.Add(ip, geoDto);

            return geoDto;
        }
    }
}
