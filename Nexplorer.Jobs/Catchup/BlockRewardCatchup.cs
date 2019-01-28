using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Data.Context;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Jobs.Catchup
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
            tIo.[TransactionInputOutputType],
            tIo.[AddressId]
            FROM [dbo].[TransactionInputOutput] tIo
            WHERE tIo.[TransactionId] = @TransactionId";

        private const string TxUpdateSql = @"
            UPDATE [dbo].[Transaction]  
            SET
            [TransactionType] = @TransactionType
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
            var currentHeight = 0;

            while (currentHeight <= dbHeight)
            {
                _stopwatch.Restart();

                using (var con = new SqlConnection(Settings.Connection.GetNexusDbConnectionString()))
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

                                var transactionType = TransactionType.User;

                                if (o == 0)
                                {
                                    var channel = (BlockChannels) tx.Channel;

                                    if (channel == BlockChannels.PoS)
                                    {
                                        var insOuts = (await con.QueryAsync(TxInOutSelectSql, new {tx.TransactionId}, trans)).ToList();

                                        var ins = insOuts.Where(x => x.TransactionInputOutputType == (int) TransactionInputOutputType.Input).ToList();
                                        var outs = insOuts.Where(x => x.TransactionInputOutputType == (int) TransactionInputOutputType.Output).ToList();

                                        if (ins.Any() && outs.Any() && outs.Count == 1)
                                        {
                                            transactionType = ins.Any(x => outs.First().AddressId == x.AddressId)
                                                ? TransactionType.Coinstake
                                                : TransactionType.CoinstakeGenesis;
                                        }
                                    }
                                    else
                                    {
                                        transactionType = channel == (BlockChannels) BlockChannels.Hash
                                            ? TransactionType.CoinbaseHash
                                            : TransactionType.CoinbasePrime;
                                    }
                                }

                                await con.ExecuteAsync(TxUpdateSql, new { tx.TransactionId, TransactionType = transactionType }, trans);

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
}