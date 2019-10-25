using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexplorer.Connect.Hub.Core;

namespace Nexplorer.Connect.Hub
{
    public class HubFactory : IHubFactory
    {
        private readonly ILogger<HubFactory> _logger;
        private readonly Dictionary<string, IHubClient> _hubs;

        public HubFactory(AppSettings settings, IServiceProvider serviceProvider, ILogger<HubFactory> logger)
        {
            _logger = logger;
            _hubs = settings.Hubs
                .ToDictionary(
                    x => x.Name, 
                    y => (IHubClient)new HubClient(y, serviceProvider.GetService<ILogger<IHubClient>>())
                );
        }

        public IHubClient Get(string name)
        {
            if (!_hubs.ContainsKey(name))
                _logger.LogError($"Hub {name} was not found");

            return _hubs[name];
        }
    }
}