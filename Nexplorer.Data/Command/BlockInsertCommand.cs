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
    public class BlockDeleteCommand
    {
        private const string TxInOutDeleteSql = @"
            DELETE tIo 
            FROM [dbo].[TransactionInputOutput] tIo
            INNER JOIN [dbo].[Transaction] t on t.[TransactionId] = tIo.[TransactionId]
            WHERE t.[BlockHeight] = @Height";

        private const string AddressDeleteSql = @"
            DELETE FROM [dbo].[Address] WHERE [dbo].[Address].[AddressId] = @AddressId";

        private const string BlockDeleteSql = @"
            DELETE FROM [dbo].[Block] WHERE [dbo].[Block].[Height] = @Height";

        private const string AddressSelectSql = @"
            SELECT 
                *
            FROM [dbo].[Address] a
            WHERE a.[AddressId] = @AddressId";

        private const string AddressFirstBlockSql = @"
            SELECT 
                MIN(t.[BlockHeight])
            FROM [dbo].[TransactionInputOutput] tIo
            INNER JOIN [dbo].[Transaction] t on t.[TransactionId] = tIo.[TransactionId]
            WHERE t.[BlockHeight] <> @Height
            AND tIo.[AddressId] = @AddressId";

        private const string AddressUpdateSql = @"
            UPDATE [dbo].[Address]  
            SET                  
                [dbo].[Address].[FirstBlockHeight] = @FirstBlockHeight
            WHERE [dbo].[Address].[AddressId] = @AddressId";

        public async Task DeleteBlockAsync(BlockDto blockDto)
        {
            using (var con = new SqlConnection(Settings.Connection.NexusDb))
            {
                await con.OpenAsync();

                using (var trans = con.BeginTransaction())
                {
                    await con.ExecuteAsync(TxInOutDeleteSql, new { blockDto.Height }, trans);

                    await DeleteOrUpdateAddressesAsync(con, trans, blockDto);

                    await con.ExecuteAsync(BlockDeleteSql, new { blockDto.Height }, trans);

                    trans.Commit();
                }
            }
        }

        private static async Task DeleteOrUpdateAddressesAsync(IDbConnection con, IDbTransaction trans, BlockDto blockDto)
        {
            var addressIds = blockDto.Transactions
                .SelectMany(x => x.Inputs.Concat(x.Outputs)
                    .Select(y => y.AddressId));

            foreach (var addressId in addressIds)
            {
                var address = (await con.QueryAsync<Address>(AddressSelectSql, new { AddressId = addressId }, trans)).FirstOrDefault();

                if (address == null || address.FirstBlockHeight != blockDto.Height)
                    continue;

                var newFirstBlockHeight = (await con.QueryAsync<int>(AddressFirstBlockSql, new { blockDto.Height, AddressId = addressId }, trans)).FirstOrDefault();

                if (newFirstBlockHeight == 0)
                    await con.ExecuteAsync(AddressDeleteSql, new { AddressId = addressId }, trans);
                else
                {
                    address.FirstBlockHeight = newFirstBlockHeight;
                    await con.ExecuteAsync(AddressUpdateSql, new { address.FirstBlockHeight, AddressId = addressId }, trans);
                }
            }
        }
    }

    public class BlockInsertCommand
    {
        private const string BlockInsertSql = @"
            INSERT INTO [dbo].[Block] ([Height], [Bits], [Channel], [Difficulty], [Hash], [MerkleRoot], [Mint], [Nonce], [Size], [Timestamp], [Version]) 
            VALUES (@Height, @Bits, @Channel, @Difficulty, @Hash, @MerkleRoot, @Mint, @Nonce, @Size, @Timestamp, @Version);";

        private const string TxInsertSql = @"
            INSERT INTO [dbo].[Transaction] ([Amount], [BlockHeight], [Hash], [Timestamp], [TransactionType]) 
            VALUES (@Amount, @BlockHeight, @Hash, @Timestamp, @TransactionType);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        private const string AddressInsertSql = @"
            INSERT INTO [dbo].[Address] ([Hash], [FirstBlockHeight]) 
            VALUES (@Hash, @BlockHeight);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        private const string TxInOutInsertSql = @"
            INSERT INTO [dbo].[TransactionInputOutput] ([TransactionId], [AddressId], [TransactionInputOutputType], [Amount]) 
            VALUES (@TransactionId, @AddressId, @TransactionInputOutputType, @Amount);";

        private const string AddressSelectSql = @"
            SELECT a.AddressId
            FROM [dbo].[Address] a
            WHERE a.Hash = @Hash";

        public Task<List<Block>> InsertBlockAsync(BlockDto blockDto, bool consoleOutput = false)
        {
            return InsertBlocksAsync(new List<BlockDto>() {blockDto});
        }

        public async Task<List<Block>> InsertBlocksAsync(List<BlockDto> blockDtos, bool consoleOutput = false)
        {
            var blocks = new List<Block>();

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

                        block.Transactions = MapTransactions(blockDto.Transactions).ToList();

                        var txInOutDtos = blockDto.Transactions.SelectMany(x => x.Inputs.Concat(x.Outputs)).ToList();
                        var addressesCache = await InsertAddressesAsync(con, trans, txInOutDtos, block.Height);
                        var txIds = await InsertTransactionsAsync(con, trans, block.Transactions);

                        var txInsOuts = blockDto.Transactions
                            .SelectMany((x, i) => MapTransactionInputOutputs(x, addressesCache, txIds[i]))
                            .ToList();

                        await InsertTransactionInputOutputsAsync(con, trans, txInsOuts);

                        for (var i = 0; i < block.Transactions.Count; i++)
                        {
                            var tx = block.Transactions[i];

                            tx.TransactionId = txIds[i];
                            tx.InputOutputs = txInsOuts.Where(x => x.TransactionId == tx.TransactionId).ToList();
                        }

                        blocks.Add(block);

                        if (consoleOutput)
                            LogProgress(index + 1, blockDtos.Count);
                    }

                    trans.Commit();

                    if (consoleOutput)
                        Console.WriteLine();
                }
            }

            return blocks;
        }

        private static void LogProgress(int i, int total)
        {
            var pct = ((double) i / total) * 100;

            var progress = Math.Floor((double) pct / 5);
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

        private static async Task<List<int>> InsertTransactionsAsync(IDbConnection sqlCon, IDbTransaction trans,
            IEnumerable<Transaction> txs)
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
                    TimeStamp = timestamp,
                    tx.TransactionType
                }, trans);

                txIds.Add(result.Single());
            }

            return txIds;
        }

        private static async Task<Dictionary<string, int>> InsertAddressesAsync(IDbConnection sqlCon,
            IDbTransaction trans, IEnumerable<TransactionInputOutputDto> txInOuts, int blockHeight)
        {
            var addressCache = new Dictionary<string, int>();
            var addressHashes = txInOuts.Select(y => y.AddressHash).Distinct();

            foreach (var addressHash in addressHashes)
            {
                var selectResult = await sqlCon.QueryAsync<int>(AddressSelectSql, new {Hash = addressHash}, trans);

                var id = selectResult.FirstOrDefault();

                if (id == 0)
                {
                    var insertResult = await sqlCon.QueryAsync<int>(AddressInsertSql,
                        new {Hash = addressHash, BlockHeight = blockHeight}, trans);

                    id = insertResult.Single();
                }

                addressCache.Add(addressHash, id);
            }

            return addressCache;
        }

        private static async Task InsertTransactionInputOutputsAsync(IDbConnection sqlCon, IDbTransaction trans,
            IEnumerable<TransactionInputOutput> txInOuts)
        {
            await sqlCon.ExecuteAsync(TxInOutInsertSql, txInOuts.Select(x => new
            {
                x.TransactionId,
                x.AddressId,
                x.TransactionInputOutputType,
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
            return txDtos.Select((x, i) => new Transaction
            {
                Amount = x.Amount,
                BlockHeight = x.BlockHeight,
                Hash = x.Hash,
                Timestamp = x.Timestamp,
                TransactionType = x.TransactionType
            });
        }

        private static IEnumerable<TransactionInputOutput> MapTransactionInputOutputs(TransactionDto txDto,
            IReadOnlyDictionary<string, int> addressCache, int transactionId)
        {
            return txDto.Inputs
                .Select(y => new TransactionInputOutput
                {
                    TransactionId = transactionId,
                    Amount = y.Amount,
                    TransactionInputOutputType = TransactionInputOutputType.Input,
                    AddressId = addressCache[y.AddressHash]
                }).Concat(txDto.Outputs
                    .Select(y => new TransactionInputOutput
                    {
                        TransactionId = transactionId,
                        Amount = y.Amount,
                        TransactionInputOutputType = TransactionInputOutputType.Output,
                        AddressId = addressCache[y.AddressHash]
                    }));
        }
    }
}
