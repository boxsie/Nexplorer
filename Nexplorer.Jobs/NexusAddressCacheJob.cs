using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Jobs.Service;

namespace Nexplorer.Jobs
{
    public class NexusAddressCacheJob : HostedService
    {
        private readonly ILogger<NexusAddressCacheJob> _logger;
        private readonly AddressQuery _addressQuery;
        private readonly RedisCommand _redisCommand;

        public NexusAddressCacheJob(ILogger<NexusAddressCacheJob> logger, AddressQuery addressQuery, RedisCommand redisCommand) 
            : base(180)
        {
            _logger = logger;
            _addressQuery = addressQuery;
            _redisCommand = redisCommand;
        }

        protected override async Task ExecuteAsync()
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
        }
    }
}