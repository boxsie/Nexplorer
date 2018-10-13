using System;
using System.Collections.Generic;
using System.Linq;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;
using Nexplorer.Domain.Models;

namespace Nexplorer.Web.Models
{
    public class BlockViewModel
    {
        public int Height { get; set; }
        public int ChannelHeight { get; set; }
        public int Confirmations { get; set; }
        public int Size { get; set; }
        public string Channel { get; set; }
        public int Version { get; set; }
        public DateTime TimeUtc { get; set; }
        public double Nonce { get; set; }
        public string Bits { get; set; }
        public double Difficulty { get; set; }
        public double Mint { get; set; }
        public double TransactionsTotal { get; set; }
        public bool HasNextBlock { get; set; }
        public string Hash { get; set; }
        public string MerkleRoot { get; set; }

        public List<TransactionModel> Transactions { get; set; }

        public BlockViewModel(BlockDto block, int channelHeight, int confirmations)
        {
            Height = block.Height;
            ChannelHeight = channelHeight;
            Confirmations = confirmations;
            Size = block.Size;
            Channel = ((BlockChannels)block.Channel).ToString();
            Version = block.Version;
            TimeUtc = block.Timestamp;
            Nonce = block.Nonce;
            Bits = block.Bits;
            Difficulty = block.Difficulty;
            Mint = block.Mint;
            HasNextBlock = confirmations > 1;
            Hash = block.Hash;
            MerkleRoot = block.MerkleRoot;

            Transactions = block.Transactions?.Select(x => new TransactionModel(x)).ToList()
                           ?? new List<TransactionModel>();

            TransactionsTotal = Transactions.Sum(x => x.TransactionOutputs.Sum(y => y.Amount));
        }
    }
}