using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Nexplorer.Config;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Data.Command
{
    public class AddressAggregatorCommand
    {
        private const string BlockTxSelectSql = @"
            SELECT
	            txInOut.[TransactionInputOutputType],
	            t.[BlockHeight],
	            txInOut.[Amount],	
	            txInOut.[AddressId]
            FROM [dbo].[TransactionInputOutput] txInOut
            INNER JOIN [dbo].[Address] a ON a.AddressId = txInOut.AddressId
            INNER JOIN [dbo].[Transaction] t ON t.TransactionId = txInOut.TransactionId
            INNER JOIN [dbo].[Block] b ON b.Height = t.BlockHeight
            WHERE b.[Height] = @BlockHeight";

        private const string AddressAggregateSelectSql = @"
            SELECT    
                a.[AddressId],
	            a.[LastBlockHeight],
	            a.[Balance],
	            a.[ReceivedAmount],
	            a.[ReceivedCount],
	            a.[SentAmount],
	            a.[SentCount],
	            a.[UpdatedOn] 
            FROM [dbo].[AddressAggregate] a
            WHERE a.[AddressId] = @AddressId";

        private const string AddressAggregateInsertSql = @"
            INSERT INTO [dbo].[AddressAggregate] ([AddressId], [LastBlockHeight], [Balance], [ReceivedAmount], [ReceivedCount], [SentAmount], [SentCount], [UpdatedOn]) 
            VALUES (@AddressId, @LastBlockHeight, @Balance, @ReceivedAmount, @ReceivedCount, @SentAmount, @SentCount, @UpdatedOn);";

        private const string AddressAggregateUpdateSql = @"
            UPDATE [dbo].[AddressAggregate]  
            SET 
                [LastBlockHeight] = @LastBlockHeight, 
                [Balance] = @Balance, 
                [ReceivedAmount] = @ReceivedAmount, 
                [ReceivedCount] = @ReceivedCount, 
                [SentAmount] = @SentAmount, 
                [SentCount] = @SentCount, 
                [UpdatedOn] = @UpdatedOn
            WHERE [AddressId] = @AddressId";

        private const string AddressAggregateDeleteSql = @"
            DELETE FROM [dbo].[AddressAggregate] 
            WHERE [dbo].[AddressAggregate].[AddressId] = @AddressId";

        private const string PreviousLastBlockSql = @"
            SELECT
                MAX(b.[Height])
            FROM [dbo].[TransactionInputOutput] txInOut
            INNER JOIN [dbo].[Address] a ON a.AddressId = txInOut.AddressId
            INNER JOIN [dbo].[Transaction] t ON t.TransactionId = txInOut.TransactionId
            INNER JOIN [dbo].[Block] b ON b.Height = t.BlockHeight
            WHERE a.[AddressId] = @AddressId
            AND b.[Height] <> @Height";

        private readonly Dictionary<int, AddressAggregate> _addressAggregates;

        public AddressAggregatorCommand()
        {
            _addressAggregates = new Dictionary<int, AddressAggregate>();
        }

        public Task AggregateAddressesAsync(BlockDto block)
        {
            return AggregateAddressesAsync(new List<BlockDto> {block});
        }

        public async Task AggregateAddressesAsync(IEnumerable<BlockDto> blocks)
        {
            _addressAggregates.Clear();

            using (var con = new SqlConnection(Settings.Connection.GetNexusDbConnectionString()))
            {
                await con.OpenAsync();

                using (var trans = con.BeginTransaction())
                {
                    foreach (var block in blocks)
                    {
                        var txIos = await con.QueryAsync<TransactionInputOutput>(BlockTxSelectSql, new { BlockHeight = block.Height }, trans);

                        foreach (var txIo in txIos)
                            await UpdateOrInsertAggregateAsync(con, trans, txIo, block.Height);
                    }

                    trans.Commit();
                }
            }
        }

        public async Task AggregateAddresses(int startHeight, int count, bool consoleOutput = false)
        {
            using (var con = new SqlConnection(Settings.Connection.GetNexusDbConnectionString()))
            {
                await con.OpenAsync();

                using (var trans = con.BeginTransaction())
                {
                    for (var i = startHeight; i < startHeight + count; i++)
                    {
                        var txIos = await con.QueryAsync<TransactionInputOutput>(BlockTxSelectSql, new { BlockHeight = i }, trans);

                        foreach (var txIo in txIos)
                            await UpdateOrInsertAggregateAsync(con, trans, txIo, i);

                        if (consoleOutput)
                            LogProgress((i - startHeight) + 1, count);
                    }

                    trans.Commit();
                }
            }
        }

        public async Task RevertAggregate(BlockDto block)
        {
            if (block?.Transactions == null || !block.Transactions.Any())
                return;

            using (var con = new SqlConnection(Settings.Connection.GetNexusDbConnectionString()))
            {
                await con.OpenAsync();

                using (var trans = con.BeginTransaction())
                {
                    var txIos = block.Transactions.SelectMany(x => x.Inputs.Concat(x.Outputs));

                    foreach (var txIo in txIos)
                    {
                        var response = (await con.QueryAsync<AddressAggregate>(AddressAggregateSelectSql, new { txIo.AddressId }, trans)).FirstOrDefault();
                        
                        if (response == null)
                            continue;

                        var previousLastHeight = (await con.QueryAsync<int?>(PreviousLastBlockSql, new { txIo.AddressId, block.Height }, trans)).FirstOrDefault();

                        if (previousLastHeight.HasValue)
                        {
                            response.RevertAggregateProperties(txIo.TransactionInputOutputType, txIo.Amount, previousLastHeight.Value);
                            await UpdateOrInsertAggregateAsync(con, trans, response, false);
                        }
                        else
                            await con.ExecuteAsync(AddressAggregateDeleteSql, new { txIo.AddressId }, trans);
                    }

                    trans.Commit();
                }
            }
        }

        private async Task UpdateOrInsertAggregateAsync(IDbConnection sqlCon, IDbTransaction trans, TransactionInputOutput txIo, int blockHeight)
        {
            AddressAggregate addAgg;

            if (_addressAggregates.ContainsKey(txIo.AddressId))
            {
                addAgg = _addressAggregates[txIo.AddressId];

                addAgg.ModifyAggregateProperties(txIo.TransactionInputOutputType, txIo.Amount, blockHeight);

                await UpdateOrInsertAggregateAsync(sqlCon, trans, addAgg, false);
            }
            else
            {
                var response = (await sqlCon.QueryAsync<AddressAggregate>(AddressAggregateSelectSql, new { txIo.AddressId }, trans)).ToList();

                var isNew = !response.Any();

                addAgg = isNew
                    ? new AddressAggregate { AddressId = txIo.AddressId }
                    : response.First();

                addAgg.ModifyAggregateProperties(txIo.TransactionInputOutputType, txIo.Amount, blockHeight);

                await UpdateOrInsertAggregateAsync(sqlCon, trans, addAgg, isNew);

                _addressAggregates.Add(addAgg.AddressId, addAgg);
            }
        }

        private static async Task UpdateOrInsertAggregateAsync(IDbConnection sqlCon, IDbTransaction trans, AddressAggregate addAgg, bool isInsert)
        {
            await sqlCon.ExecuteAsync(isInsert ? AddressAggregateInsertSql : AddressAggregateUpdateSql, new
            {
                addAgg.AddressId,
                addAgg.LastBlockHeight,
                addAgg.Balance,
                addAgg.ReceivedAmount,
                addAgg.ReceivedCount,
                addAgg.SentAmount,
                addAgg.SentCount,
                UpdatedOn = DateTime.Now
            }, trans);
        }

        private static void LogProgress(int i, int total)
        {
            var pct = ((double)i / total) * 100;

            var progress = Math.Floor(pct / 5);
            var bar = "";

            for (var o = 0; o < 20; o++)
            {
                bar += progress > o
                    ? '#'
                    : ' ';
            }

            Console.Write($"\rSaving address aggregate updates to database... [{bar}] {pct:N2}%   ");
        }
    }
}