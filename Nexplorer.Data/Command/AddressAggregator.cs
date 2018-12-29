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
    public class AddressAggregator : IDisposable
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

        private readonly Dictionary<int, AddressAggregate> _addressAggregates;

        public AddressAggregator()
        {
            _addressAggregates = new Dictionary<int, AddressAggregate>();

        }

        public async Task AggregateAddresses(int startHeight, int count)
        {
            using (var con = new SqlConnection(Settings.Connection.NexusDb))
            {
                await con.OpenAsync();

                using (var trans = con.BeginTransaction())
                {
                    for (var i = startHeight; i < startHeight + count; i++)
                    {
                        var txIoDtos = await con.QueryAsync<TransactionInputOutputDto>(BlockTxSelectSql, new {BlockHeight = i}, trans);

                        foreach (var txIoDto in txIoDtos)
                            await UpdateOrInsertAggregate(con, trans, txIoDto);


                        LogProgress((i - startHeight) + 1, count);
                    }

                    trans.Commit();
                }
            }
        }

        public async Task AggregateAddresses(IEnumerable<Block> blocks)
        {
            using (var con = new SqlConnection(Settings.Connection.NexusDb))
            {
                await con.OpenAsync();

                using (var trans = con.BeginTransaction())
                {
                    var txIoDtos = blocks
                        .SelectMany(x => x.Transactions
                            .SelectMany(y => y.InputOutputs.Select(z => new { TransactionIoType = z.TransactionInputOutputType, z.AddressId, z.Amount })
                                .Select(z => new TransactionInputOutputDto
                                {
                                    AddressId = z.AddressId,
                                    Amount = z.Amount,
                                    BlockHeight = x.Height,
                                    TransactionInputOutputType = z.TransactionIoType
                                })))
                        .ToList();

                    for (var i = 0; i < txIoDtos.Count; i++)
                    {
                        var txIoDto = txIoDtos[i];

                        await UpdateOrInsertAggregate(con, trans, txIoDto);

                        LogProgress(i + 1, txIoDtos.Count);
                    }

                    trans.Commit();
                }
            }
        }

        private async Task UpdateOrInsertAggregate(IDbConnection sqlCon, IDbTransaction trans, TransactionInputOutputDto txIo)
        {
            AddressAggregate addAgg;

            if (_addressAggregates.ContainsKey(txIo.AddressId))
            {
                addAgg = _addressAggregates[txIo.AddressId];

                addAgg.ModifyAggregateProperties(txIo.TransactionInputOutputType, txIo.Amount, txIo.BlockHeight);

                await UpdateOrInsertAggregate(sqlCon, trans, addAgg, false);
            }
            else
            {
                var response = (await sqlCon.QueryAsync<AddressAggregate>(AddressAggregateSelectSql, new { txIo.AddressId }, trans)).ToList();

                var isNew = !response.Any();

                addAgg = isNew
                    ? new AddressAggregate { AddressId = txIo.AddressId }
                    : response.First();

                addAgg.ModifyAggregateProperties(txIo.TransactionInputOutputType, txIo.Amount, txIo.BlockHeight);

                await UpdateOrInsertAggregate(sqlCon, trans, addAgg, isNew);

                _addressAggregates.Add(addAgg.AddressId, addAgg);
            }
        }

        private static async Task UpdateOrInsertAggregate(IDbConnection sqlCon, IDbTransaction trans, AddressAggregate addAgg, bool isInsert)
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

            var progress = Math.Floor((double)pct / 5);
            var bar = "";

            for (var o = 0; o < 20; o++)
            {
                bar += progress > o
                    ? '#'
                    : ' ';
            }

            Console.Write($"\rSaving address aggregate updates to database... [{bar}] {pct:N2}%   ");
        }

        public void Dispose()
        {
            _addressAggregates.Clear();
        }
    }
}