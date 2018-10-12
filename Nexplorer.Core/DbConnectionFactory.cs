using System.Data.Common;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Nexplorer.Config;

namespace Nexplorer.Core
{
    public static class DbConnectionFactory
    {
        public static Task<DbConnection> GetNexusDbConnectionAsync()
        {
            return GetOpenDbConnectionAsync(Settings.Connection.NexusDb);
        }

        public static Task<DbConnection> GetNexplorerDbConnectionAsync()
        {
            return GetOpenDbConnectionAsync(Settings.Connection.NexusDb);
        }

        private static async Task<DbConnection> GetOpenDbConnectionAsync(string connectionString)
        {
            var connection = new MySqlConnection(connectionString);

            await connection.OpenAsync();

            return connection;
        }
    }
}