using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Data.Command
{
    public static class AddressAggregator
    {
        private const string BlockTxSelectSql = @"
            SELECT
	            1 AS TxType,
	            t.[BlockHeight],
	            txIn.[Amount],
	            txIn.[AddressId]
            FROM [dbo].[TransactionInput] txIn
            INNER JOIN [dbo].[Address] a ON a.AddressId = txIn.AddressId
            INNER JOIN [dbo].[Transaction] t ON t.TransactionId = txIn.TransactionId
            INNER JOIN [dbo].[Block] b ON b.Height = t.BlockHeight
            WHERE b.[Height] = @BlockHeight
            UNION ALL
            SELECT
	            2 AS TxType,
	            t.[BlockHeight],
	            txOut.[Amount],	
	            txOut.[AddressId]
            FROM [dbo].[TransactionOutput] txOut
            INNER JOIN [dbo].[Address] a ON a.AddressId = txOut.AddressId
            INNER JOIN [dbo].[Transaction] t ON t.TransactionId = txOut.TransactionId
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

        private static readonly Dictionary<int, AddressAggregate> _addressAggregates;

        static AddressAggregator()
        {
            _addressAggregates = new Dictionary<int, AddressAggregate>();
        }

        public static async Task AggregateAddresses(int startHeight, int count)
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

        public static async Task AggregateAddresses(this List<Block> blocks)
        {
            using (var con = new SqlConnection(Settings.Connection.NexusDb))
            {
                await con.OpenAsync();

                using (var trans = con.BeginTransaction())
                {
                    var txIoDtos = blocks
                        .SelectMany(x => x.Transactions
                            .SelectMany(y => y.Inputs
                                .Select(z => new { TxType = TransactionType.Input, z.AddressId, z.Amount })
                                .Concat(y.Outputs
                                    .Select(z => new { TxType = TransactionType.Output, z.AddressId, z.Amount}))
                                .Select(z => new TransactionInputOutputDto
                                {
                                    AddressId = z.AddressId,
                                    Amount = z.Amount,
                                    BlockHeight = x.Height,
                                    TxType = z.TxType
                                })))
                        .ToList();

                    for (var i = 0; i < txIoDtos.Count; i++)
                    {
                        var txIoDto = txIoDtos[i];

                        await UpdateOrInsertAggregate(con, trans, txIoDto);

                        LogProgress(i, txIoDtos.Count);
                    }

                    trans.Commit();
                }
            }
        }

        private static async Task UpdateOrInsertAggregate(IDbConnection sqlCon, IDbTransaction trans, TransactionInputOutputDto txIo)
        {
            AddressAggregate addAgg;

            if (_addressAggregates.ContainsKey(txIo.AddressId))
            {
                addAgg = _addressAggregates[txIo.AddressId];

                addAgg.ModifyAggregateProperties(txIo.TxType, txIo.Amount, txIo.BlockHeight);

                await UpdateOrInsertAggregate(sqlCon, trans, addAgg, false);
            }
            else
            {
                var response = (await sqlCon.QueryAsync<AddressAggregate>(AddressAggregateSelectSql, new { txIo.AddressId }, trans)).ToList();

                var isNew = !response.Any();

                addAgg = isNew
                    ? new AddressAggregate { AddressId = txIo.AddressId }
                    : response.First();

                addAgg.ModifyAggregateProperties(txIo.TxType, txIo.Amount, txIo.BlockHeight);

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
    }

    public static class BlockInsert
    {
        private const string BlockInsertSql = @"
            INSERT INTO [dbo].[Block] ([Height], [Bits], [Channel], [Difficulty], [Hash], [MerkleRoot], [Mint], [Nonce], [Size], [Timestamp], [Version]) 
            VALUES (@Height, @Bits, @Channel, @Difficulty, @Hash, @MerkleRoot, @Mint, @Nonce, @Size, @Timestamp, @Version);";

        private const string TxInsertSql = @"
            INSERT INTO [dbo].[Transaction] ([Amount], [BlockHeight], [Hash], [Timestamp]) 
            VALUES (@Amount, @BlockHeight, @Hash, @Timestamp);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        private const string AddressInsertSql = @"
            INSERT INTO [dbo].[Address] ([Hash], [FirstBlockHeight]) 
            VALUES (@Hash, @BlockHeight);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        private const string TxInInsertSql = @"
            INSERT INTO [dbo].[TransactionInput] ([TransactionId], [AddressId], [Amount]) 
            VALUES (@TransactionId, @AddressId, @Amount);";

        private const string TxOutInsertSql = @"
            INSERT INTO [dbo].[TransactionOutput] ([TransactionId], [AddressId], [Amount]) 
            VALUES (@TransactionId, @AddressId, @Amount);";

        private const string AddressSelectSql = @"
            SELECT a.AddressId
            FROM [dbo].[Address] a
            WHERE a.Hash = @Hash";
        
        public static async Task InsertBlocksAsync(this List<BlockDto> blockDtos)
        {
            using (var con = new SqlConnection(Settings.Connection.NexusDb))
            {
                await con.OpenAsync();

                using (var trans = con.BeginTransaction())
                {
                    for (var index = 0; index < blockDtos.Count; index++)
                    {
                        var blockDto = blockDtos[index];

                        var block = MapBlock(blockDto);
                        await InsertBlockAsync(con, trans, block);

                        var txs = MapTransactions(blockDto.Transactions);
                        var txIds = await InsertTransactionsAsync(con, trans, txs);

                        var txInOutDtos = blockDto.Transactions.SelectMany(x => x.Inputs.Concat(x.Outputs)).ToList();

                        var addressesCache = await InsertAddressesAsync(con, trans, txInOutDtos, block.Height);

                        var txInsOuts = blockDto.Transactions
                            .Select((x, i) => MapTransactionInputOutputs(x, addressesCache, txIds[i]))
                            .ToList();

                        await InsertTransactionInputsAsync(con, trans, txInsOuts.SelectMany(x => x.Item1));
                        await InsertTransactionOutputsAsync(con, trans, txInsOuts.SelectMany(x => x.Item2));

                        LogProgress(index, blockDtos.Count - 1);
                    }

                    trans.Commit();
                }
            }
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

            Console.Write($"\rSaving blocks to database... [{bar}] {pct:N2}%   ");
        }

        private static async Task InsertBlockAsync(IDbConnection sqlCon, IDbTransaction trans, Block block)
        {
            await sqlCon.ExecuteAsync(BlockInsertSql, new
            {
                block.Height,
                block.Bits,
                block.Channel,
                block.Difficulty,
                block.Hash,
                block.MerkleRoot,
                block.Mint,
                block.Nonce,
                block.Size,
                block.Timestamp,
                block.Version
            }, trans);
        }

        private static async Task<List<int>> InsertTransactionsAsync(IDbConnection sqlCon, IDbTransaction trans, IEnumerable<Transaction> txs)
        {
            var txIds = new List<int>();

            foreach (var tx in txs)
            {
                var timestamp = tx.Timestamp == DateTime.MinValue
                    ? new DateTime(1970, 1, 1)
                    : tx.Timestamp;

                var result = await sqlCon.QueryAsync<int>(TxInsertSql, new
                {
                    tx.Amount,
                    tx.BlockHeight,
                    tx.Hash,
                    TimeStamp = timestamp
                }, trans);

                txIds.Add(result.Single());
            }

            return txIds;
        }

        private static async Task<Dictionary<string, int>> InsertAddressesAsync(IDbConnection sqlCon, IDbTransaction trans, IEnumerable<TransactionInputOutputLiteDto> txInOuts, int blockHeight)
        {
            var addressCache = new Dictionary<string, int>();
            var addressHashes = txInOuts.Select(y => y.AddressHash).Distinct();

            foreach (var addressHash in addressHashes)
            {
                var selectResult = await sqlCon.QueryAsync<int>(AddressSelectSql, new { Hash = addressHash }, trans);

                var id = selectResult.FirstOrDefault();

                if (id == 0)
                {
                    var insertResult = await sqlCon.QueryAsync<int>(AddressInsertSql, new { Hash = addressHash, BlockHeight = blockHeight }, trans);

                    id = insertResult.Single();
                }

                addressCache.Add(addressHash, id);
            }

            return addressCache;
        }

        private static async Task InsertTransactionInputsAsync(IDbConnection sqlCon, IDbTransaction trans, IEnumerable<TransactionInput> txIns)
        {
            await sqlCon.ExecuteAsync(TxInInsertSql, txIns.Select(x => new
            {
                x.TransactionId,
                x.AddressId,
                x.Amount
            }), trans);
        }

        private static async Task InsertTransactionOutputsAsync(IDbConnection sqlCon, IDbTransaction trans, IEnumerable<TransactionOutput> txOuts)
        {
            await sqlCon.ExecuteAsync(TxOutInsertSql, txOuts.Select(x => new
            {
                x.TransactionId,
                x.AddressId,
                x.Amount
            }), trans);
        }

        private static Block MapBlock(BlockDto blockDto)
        {
            return new Block
            {
                Height = blockDto.Height,
                Bits = blockDto.Bits,
                Channel = blockDto.Channel,
                Difficulty = blockDto.Difficulty,
                Hash = blockDto.Hash,
                MerkleRoot = blockDto.MerkleRoot,
                Mint = blockDto.Mint,
                Nonce = blockDto.Nonce,
                Size = blockDto.Size,
                Timestamp = blockDto.Timestamp,
                Version = blockDto.Version
            };
        }

        private static IEnumerable<Transaction> MapTransactions(IEnumerable<TransactionDto> txDtos)
        {
            return txDtos.Select(x => new Transaction
            {
                Amount = x.Amount,
                BlockHeight = x.BlockHeight,
                Hash = x.Hash,
                Timestamp = x.Timestamp
            });
        }

        private static Tuple<IEnumerable<TransactionInput>, IEnumerable<TransactionOutput>> MapTransactionInputOutputs(
            TransactionDto txDto, IReadOnlyDictionary<string, int> addressCache, int transactionId)
        {
            return new Tuple<IEnumerable<TransactionInput>, IEnumerable<TransactionOutput>>(
                txDto.Inputs.Select(y => new TransactionInput
                {
                    TransactionId = transactionId,
                    Amount = y.Amount,
                    AddressId = addressCache[y.AddressHash]
                }),
                txDto.Outputs.Select(y => new TransactionOutput
                {
                    TransactionId = transactionId,
                    Amount = y.Amount,
                    AddressId = addressCache[y.AddressHash]
                }));
        }
    }
}
