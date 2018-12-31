using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Tools.Jobs
{
    public class NexusAddressCacheJob
    {
        public static readonly TimeSpan JobInterval = TimeSpan.FromMinutes(3);

        private readonly ILogger<NexusAddressCacheJob> _logger;
        private readonly AddressQuery _addressQuery;
        private readonly RedisCommand _redisCommand;

        private const int TimeoutSeconds = 10;

        public NexusAddressCacheJob(ILogger<NexusAddressCacheJob> logger, AddressQuery addressQuery, RedisCommand redisCommand)
        {
            _logger = logger;
            _addressQuery = addressQuery;
            _redisCommand = redisCommand;
        }

        public async Task CacheNexusAddressesAsync()
        {
            var nexusAddresses = new List<AddressLiteDto>();

            var hashes = AddressQuery.NexusAddresses.Values.SelectMany(x => x.SelectMany(y => y));

            foreach (var nexusAmbassadorAddress in hashes)
            {
                var address = await _addressQuery.GetAddressLiteAsync(nexusAmbassadorAddress);

                address.IsNexus = true;

                nexusAddresses.Add(address);
            }

            await _redisCommand.SetAsync(Settings.Redis.NexusAddressCache, nexusAddresses);

            _logger.LogInformation($"{nexusAddresses.Count} Nexus addresses updated");

            BackgroundJob.Schedule<NexusAddressCacheJob>(x => x.CacheNexusAddressesAsync(), JobInterval);
        }
    }
}