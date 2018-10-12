using System;
using System.Collections.Generic;
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
        public DateTime TimeUtc { get; set; }

        [ProtoMember(6)]
        public double Amount { get; set; }

        [ProtoMember(7)]
        public List<TransactionInputOutputDto> Inputs { get; set; }

        [ProtoMember(8)]
        public List<TransactionInputOutputDto> Outputs { get; set; }

        public TransactionDto()
        {
            Inputs = new List<TransactionInputOutputDto>();
            Outputs = new List<TransactionInputOutputDto>();
        }
    }
}