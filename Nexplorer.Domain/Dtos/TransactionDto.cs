using System;
using System.Collections.Generic;
using Nexplorer.Domain.Enums;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class TransactionDto
    {
        [ProtoMember(1)]
        public int TransactionId { get; set; }
        
        [ProtoMember(2)]
        public string Hash { get; set; }

        [ProtoMember(3)]
        public int BlockHeight { get; set; }

        [ProtoMember(4)]
        public int Confirmations { get; set; }

        [ProtoMember(5)]
        public DateTime Timestamp { get; set; }

        [ProtoMember(6)]
        public double Amount { get; set; }

        [ProtoMember(7)]
        public BlockRewardType RewardType { get; set; }

        [ProtoMember(8)]
        public List<TransactionInputOutputLiteDto> Inputs { get; set; }

        [ProtoMember(9)]
        public List<TransactionInputOutputLiteDto> Outputs { get; set; }

        public TransactionDto()
        {
            Inputs = new List<TransactionInputOutputLiteDto>();
            Outputs = new List<TransactionInputOutputLiteDto>();
        }
    }
}