using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nexplorer.Nexus;

namespace Nexplorer.Core
{
    public class HealthCheckService : ScheduledService
    {
        private readonly INexusConnection _connection;

        public HealthCheckService(INexusConnection connection, ILogger<HealthCheckService> logger) : base(TimeSpan.FromSeconds(30), logger)
        {
            _connection = connection;
        }

        public override async Task Execute()
        {
            await _connection.RefreshAsync();
        }
    }
}