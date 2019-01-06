using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Data.Command;
using Nexplorer.Data.Query;

namespace Nexplorer.Jobs.Catchup
{
    public class AddressAggregateCatchup
    {
        private readonly ILogger<AddressAggregateCatchup> _logger;
        private readonly BlockQuery _blockQuery;

        private Stopwatch _stopwatch;
        private double _totalSeconds;
        private int _iterationCount;

        public AddressAggregateCatchup(ILogger<AddressAggregateCatchup> logger, BlockQuery blockQuery)
        {
            _logger = logger;
            _blockQuery = blockQuery;
        }

        public async Task CatchupAsync()
        {
            var lastBlockHeight = await GetLastBlockHeight();

            var dbHeight = await _blockQuery.GetLastSyncedHeightAsync();

            _stopwatch = new Stopwatch();
            _totalSeconds = 0;
            _iterationCount = 0;

            using (var addAgg = new AddressAggregator())
            { 
                while (lastBlockHeight < dbHeight)
                {
                    var nextBlockHeight = lastBlockHeight + 1;

                    var bulkSaveCount = Settings.App.BulkSaveCount < (dbHeight - lastBlockHeight)
                        ? Settings.App.BulkSaveCount
                        : (dbHeight - lastBlockHeight);

                    var lastHeight = dbHeight - nextBlockHeight > bulkSaveCount
                        ? nextBlockHeight + bulkSaveCount
                        : dbHeight;

                    Console.WriteLine();

                    _logger.LogInformation($"Adding address aggregate data from block {nextBlockHeight} -> {lastHeight - 1}");

                    _stopwatch.Restart();

                    Console.WriteLine($"Aggregating block addresses... {LogProgress(lastBlockHeight, dbHeight, out var blockPct)} {blockPct:N4}% ({lastBlockHeight:N0}/{dbHeight:N0})");

                    await addAgg.AggregateAddresses(nextBlockHeight, bulkSaveCount, true);

                    lastBlockHeight = await GetLastBlockHeight();

                    LogTimeTaken(dbHeight - nextBlockHeight, _stopwatch.Elapsed);
#if DEBUG
                    await Task.Delay(100);
#endif
                }
            }
        }

        private static async Task<int> GetLastBlockHeight()
        {
            const string sqlQ = "SELECT MAX(aa.[LastBlockHeight]) FROM [dbo].[AddressAggregate] aa";

            using (var connection = new SqlConnection(Settings.Connection.NexusDb))
            {
                await connection.OpenAsync();

                var height = await connection.QueryAsync<int?>(sqlQ);

                return height.FirstOrDefault() ?? 0;
            }
        }

        private void LogTimeTaken(int syncDelta, TimeSpan timeTaken)
        {
            _iterationCount++;

            _totalSeconds += timeTaken.TotalSeconds;

            var avgSeconds = _totalSeconds / _iterationCount;
            var estRemainingIterations = syncDelta / Settings.App.BulkSaveCount;

            var remainingTime = TimeSpan.FromSeconds(estRemainingIterations * avgSeconds);

            Console.WriteLine($"\nSave complete. Iteration took { timeTaken }");
            Console.WriteLine($"Estimated remaining sync time: { remainingTime }");
        }

        private static string LogProgress(int i, int total, out double pct)
        {
            pct = ((double)i / total) * 100;

            var progress = Math.Floor((double)pct / 5);
            var bar = "";

            for (var o = 0; o < 20; o++)
            {
                bar += progress > o
                    ? '#'
                    : ' ';
            }

            return $"[{bar}]";
        }
    }
}