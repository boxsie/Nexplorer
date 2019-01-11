using System;
using System.Net;
using Nexplorer.Domain.Enums;
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

        [ProtoMember(8)]
        public TransactionType TransactionType { get; set; }

        public TransactionLiteDto() { }

        public TransactionLiteDto(TransactionDto tx) : this(tx, tx.BlockHeight, tx.Confirmations) { }

        public TransactionLiteDto(TransactionDto tx, int height, int confirmations)
        {
            BlockHeight = height;
            Confirmations = confirmations;
            TransactionHash = tx.Hash;
            Amount = tx.Amount;
            Timestamp = tx.Timestamp;
            TransactionInputCount = tx.Inputs?.Count ?? 0;
            TransactionOutputCount = tx.Outputs?.Count ?? 0;
            TransactionType = tx.TransactionType;
        }
    }
}