using System;
using Nexplorer.Domain.Enums;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class BlockLiteDto
    {
        [ProtoMember(1)]
        public int Height { get; set; }

        [ProtoMember(2)]
        public string Hash { get; set; }

        [ProtoMember(3)]
        public int Size { get; set; }

        [ProtoMember(4)]
        public string Channel { get; set; }

        [ProtoMember(5)]
        public DateTime TimeUtc { get; set; }

        [ProtoMember(6)]
        public double Difficulty { get; set; }

        [ProtoMember(7)]
        public int TransactionCount { get; set; }

        public BlockLiteDto() { }
        
        public BlockLiteDto(BlockDto block)
        {
            Height = block.Height;
            Hash = block.Hash;
            Size = block.Size;
            Channel = ((BlockChannels)block.Channel).ToString();
            TimeUtc = block.TimeUtc;
            Difficulty = block.Difficulty;
            TransactionCount = block.Transactions.Count;
        }
    }
}
