using System;
using System.Net;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class TransactionLiteDto
    {
        [ProtoMember(1)]
        public int BlockHeight { get; set; }

        [ProtoMember(2)]
        public string TransactionHash { get; set; }

        [ProtoMember(3)]
        public int Confirmations { get; set; }

        [ProtoMember(4)]
        public double Amount { get; set; }

        [ProtoMember(5)]
        public DateTime Timestamp { get; set; }
        
        [ProtoMember(6)]
        public int TransactionInputCount { get; set; }

        [ProtoMember(7)]
        public int TransactionOutputCount { get; set; }

        public TransactionLiteDto() { }
        
        public TransactionLiteDto(TransactionDto tx, int height, int confirmations)
        {
            BlockHeight = height;
            TransactionHash = tx.Hash;
            Amount = tx.Amount;
            Timestamp = tx.Timestamp;
            TransactionInputCount = tx.Inputs?.Count ?? 0;
            TransactionOutputCount = tx.Outputs?.Count ?? 0;
        }
    }
}