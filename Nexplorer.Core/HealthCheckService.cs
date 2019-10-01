using System;
using System.Threading.Tasks;
using Nexplorer.Nexus;

namespace Nexplorer.Core
{
    public class HealthCheckService : ScheduledService
    {
        private readonly INexusConnection _connection;

        public HealthCheckService(INexusConnection connection) : base(TimeSpan.FromSeconds(30))
        {
            _connection = connection;
        }

        public override async Task Execute()
        {
            await _connection.RefreshAsync();
        }
    }
}