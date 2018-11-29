using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Data.Command;
using Nexplorer.Data.Context;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Sync.Nexus
{
    public class BlockRewardCatchup
    {
        private readonly NexusDb _nexusDb;
        private readonly BlockQuery _blockQuery;
        private readonly ILogger<BlockRewardCatchup> _logger;

        private Stopwatch _stopwatch;
        private double _totalSeconds;
        private int _iterationCount;

        private const string BlockSelectSqlQ = @"
            SELECT
            b.[Channel],
            t.[TransactionId]
            FROM [dbo].[Block] b
            INNER JOIN [dbo].[Transaction] t ON t.BlockHeight = b.Height
            WHERE b.[Height] = @BlockHeight";

        private const string TxInOutSelectSql = @"
            SELECT
            tIo.[TransactionType],
            tIo.[AddressId]
            FROM [dbo].[TransactionInputOutput] tIo
            WHERE tIo.[TransactionId] = @TransactionId";

        private const string TxUpdateSql = @"
            UPDATE [dbo].[Transaction]  
            SET
            [RewardType] = @RewardType
            WHERE [TransactionId] = @TransactionId";

        public BlockRewardCatchup(ILogger<BlockRewardCatchup> logger, NexusDb nexusDb, BlockQuery blockQuery)
        {
            _logger = logger;
            _nexusDb = nexusDb;
            _blockQuery = blockQuery;
        }

        public async Task Catchup()
        {
            _stopwatch = new Stopwatch();
            _totalSeconds = 0;
            _iterationCount = 0;

            var dbHeight = await _nexusDb.Blocks.CountAsync();
            var currentHeight = 1;

            while (currentHeight <= dbHeight)
            {
                _stopwatch.Restart();

                using (var con = new SqlConnection(Settings.Connection.NexusDb))
                {
                    await con.OpenAsync();

                    using (var trans = con.BeginTransaction())
                    {
                        for (var i = currentHeight; i < (currentHeight + Settings.App.BulkSaveCount); i++)
                        {
                            var txs = (await con.QueryAsync(BlockSelectSqlQ, new {BlockHeight = i}, trans)).ToList();

                            for (var o = 0; o < txs.Count; o++)
                            {
                                var tx = txs[o];

                                var rewardType = o == 0
                                    ? (BlockChannels) tx.Channel == BlockChannels.PoS
                                        ? BlockRewardType.Staking
                                        : BlockRewardType.Mining
                                    : BlockRewardType.None;

                                await con.ExecuteAsync(TxUpdateSql, new { tx.TransactionId, RewardType = rewardType }, trans);

                            }

                            Console.Write($"\rUpdating block #{i} reward tx... {LogProgress(i, dbHeight, out var blockPct)} {blockPct:N4}% ({i:N0}/{dbHeight:N0})");
                        }
                        
                        trans.Commit();

                        Console.WriteLine();

                        LogTimeTaken(dbHeight - currentHeight, _stopwatch.Elapsed);

                        currentHeight += Settings.App.BulkSaveCount;
                    }
                }
                
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

    public class AddressAggregateCatchup
    {
        private readonly NexusDb _nexusDb;
        private readonly ILogger<AddressAggregateCatchup> _logger;

        private Stopwatch _stopwatch;
        private double _totalSeconds;
        private int _iterationCount;

        public AddressAggregateCatchup(ILogger<AddressAggregateCatchup> logger, NexusDb nexusDb)
        {
            _logger = logger;
            _nexusDb = nexusDb;
        }

        public async Task Catchup()
        {
            var lastBlockHeight = await GetLastBlockHeight();

            var dbHeight = await _nexusDb.Blocks
                .OrderBy(x => x.Height)
                .Select(x => x.Height)
                .LastOrDefaultAsync();

            _stopwatch = new Stopwatch();
            _totalSeconds = 0;
            _iterationCount = 0;

            using (var addAgg = new AddressAggregator())
            { 
                while (lastBlockHeight < dbHeight)
                {
                    var nextBlockHeight = lastBlockHeight + 1;

                    var bulkSaveCount = Settings.App.BulkSaveCount;

                    var lastHeight = dbHeight - nextBlockHeight > bulkSaveCount
                        ? nextBlockHeight + bulkSaveCount
                        : dbHeight;

                    Console.WriteLine();

                    _logger.LogInformation($"Adding address aggregate data from block {nextBlockHeight} -> {lastHeight - 1}");

                    _stopwatch.Restart();

                    Console.WriteLine($"Aggregating block addresses... {LogProgress(lastBlockHeight, dbHeight, out var blockPct)} {blockPct:N4}% ({lastBlockHeight:N0}/{dbHeight:N0})");

                    await addAgg.AggregateAddresses(nextBlockHeight, bulkSaveCount);

                    lastBlockHeight = await GetLastBlockHeight();

                    LogTimeTaken(dbHeight - nextBlockHeight, _stopwatch.Elapsed);
#if DEBUG
                    await Task.Delay(1000);
#endif
                }
            }
        }

        private async Task<int> GetLastBlockHeight()
        {
            return await _nexusDb.AddressAggregates.AnyAsync()
                ? await _nexusDb.AddressAggregates.MaxAsync(x => x.LastBlockHeight)
                : 0;
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