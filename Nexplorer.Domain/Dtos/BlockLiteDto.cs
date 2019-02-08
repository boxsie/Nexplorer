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
        public int Channel { get; set; }

        [ProtoMember(5)]
        public DateTime Timestamp { get; set; }

        [ProtoMember(6)]
        public double Difficulty { get; set; }

        [ProtoMember(7)]
        public double Mint { get; set; }

        [ProtoMember(8)]
        public int TransactionCount { get; set; }

        public BlockLiteDto() { }

        public BlockLiteDto(BlockDto block)
        {
            Height = block.Height;
            Hash = block.Hash;
            Size = block.Size;
            Channel = block.Channel;
            Timestamp = block.Timestamp;
            Difficulty = block.Difficulty;
            TransactionCount = block.Transactions.Count;
        }
    }
}
