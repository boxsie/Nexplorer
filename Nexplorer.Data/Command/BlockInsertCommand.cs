﻿using System;
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

namespace Nexplorer.Data.Command
{
    public class BlockInsertCommand
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
        
        public async Task InsertBlocksAsync(IEnumerable<BlockDto> blockDtos)
        {
            using (var con = new SqlConnection(Settings.Connection.NexusDb))
            {
                await con.OpenAsync();

                using (var trans = con.BeginTransaction())
                {
                    foreach (var blockDto in blockDtos)
                    {
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
                    }

                    trans.Commit();
                }
            }
        }

        private static async Task<Dictionary<string, int>> InsertAddressesAsync(IDbConnection sqlCon, IDbTransaction trans, IEnumerable<TransactionInputOutputDto> txInOuts, int blockHeight)
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
                var result = await sqlCon.QueryAsync<int>(TxInsertSql, new
                {
                    tx.Amount,
                    tx.BlockHeight,
                    tx.Hash,
                    tx.Timestamp
                }, trans);

                txIds.Add(result.Single());
            }

            return txIds;
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
