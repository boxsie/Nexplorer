using System;
using System.Collections.Generic;
using System.Linq;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Domain.Models
{
    public class TransactionModel
    {
        public int BlockHeight { get; set; }
        public string TransactionHash { get; set; }
        public int Confirmations { get; set; }
        public double Amount { get; set; }
        public DateTime TimeUtc { get; set; }

        public List<TransactionInputOutputModel> TransactionInputs { get; set; }
        public List<TransactionInputOutputModel> TransactionOutputs { get; set; }

        public TransactionModel(TransactionDto tx)
        {
            BlockHeight = tx.BlockHeight;
            TransactionHash = tx.Hash;
            Confirmations = tx.Confirmations;
            Amount = tx.Amount;
            TimeUtc = tx.Timestamp;

            TransactionInputs = tx.Inputs?.Select(x => new TransactionInputOutputModel(x, tx.Hash)).ToList()
                                ?? new List<TransactionInputOutputModel>();

            TransactionOutputs = tx.Outputs?.Select(x => new TransactionInputOutputModel(x, tx.Hash)).ToList() 
                                 ?? new List<TransactionInputOutputModel>();
        }
    }
}