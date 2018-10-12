using System;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Domain.Models
{
    public class TransactionInputOutputModel
    {
        public string TransactionHash { get; set; }
        public string AddressHash { get; set; }
        public double Amount { get; set; }
        public DateTime SyncTime { get; set; }
        
        public TransactionInputOutputModel(TransactionInputOutputDto txInOut, string transactionHash)
        {
            TransactionHash = transactionHash;
            AddressHash = txInOut.AddressHash;
            Amount = txInOut.Amount;
        }
    }
}