using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nexplorer.Infrastructure.Core;

namespace Nexplorer.Infrastructure.Geolocate
{
    public class GeolocateIpClient : BasicClient
    {
        public GeolocateIpClient() : base("http://freegeoip.net/json/") { }

        public async Task<GeolocateResponse> GetGeolocationFromIp(string ipAddress)
        {
            return await GetAsync<GeolocateResponse>(ipAddress);
        }
    }
}
