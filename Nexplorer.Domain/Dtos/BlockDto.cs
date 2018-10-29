using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class BlockDto
    {
        [ProtoMember(1)]
        public int Height { get; set; }

        [ProtoMember(2)]
        public string Hash { get; set; }

        [ProtoMember(3)]
        public int Size { get; set; }

        [ProtoMember(4)]
        public int Channel { get; set; }

        [ProtoMember(5)]
        public int Version { get; set; }

        [ProtoMember(6)]
        public string MerkleRoot { get; set; }

        [ProtoMember(7)]
        public DateTime Timestamp { get; set; }

        [ProtoMember(8)]
        public double Nonce { get; set; }

        [ProtoMember(9)]
        public string Bits { get; set; }

        [ProtoMember(10)]
        public double Difficulty { get; set; }

        [ProtoMember(11)]
        public double Mint { get; set; }

        [ProtoMember(12)]
        public List<TransactionDto> Transactions { get; set; }
        
        [ProtoMember(13)]
        public string PreviousBlockHash { get; set; }

        [ProtoMember(14)]
        public string NextBlockHash { get; set; }

        public BlockDto()
        {
            Transactions = new List<TransactionDto>();
        }
    }
}

