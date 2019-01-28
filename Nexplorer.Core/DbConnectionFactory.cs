using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Nexplorer.Config;

namespace Nexplorer.Core
{
    public static class DbConnectionFactory
    {
        public static Task<DbConnection> GetNexusDbConnectionAsync()
        {
            return GetOpenDbConnectionAsync(Settings.Connection.GetNexusDbConnectionString());
        }

        public static Task<DbConnection> GetNexplorerDbConnectionAsync()
        {
            return GetOpenDbConnectionAsync(Settings.Connection.GetNexusDbConnectionString());
        }

        private static async Task<DbConnection> GetOpenDbConnectionAsync(string connectionString)
        {
            var connection = new SqlConnection(connectionString);

            await connection.OpenAsync();

            return connection;
        }
    }
}