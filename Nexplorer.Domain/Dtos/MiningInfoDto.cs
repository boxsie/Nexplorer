using System;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class MiningInfoDto
    {
        [ProtoMember(1)]
        public int Blocks { get; set; }

        [ProtoMember(2)]
        public DateTime TimeStampUtc { get; set; }

        [ProtoMember(3)]
        public int CurrentBlockSize { get; set; }

        [ProtoMember(4)]
        public int CurrentBlockTx { get; set; }

        [ProtoMember(5)]
        public double PrimeDifficulty { get; set; }

        [ProtoMember(6)]
        public double HashDifficulty { get; set; }

        [ProtoMember(7)]
        public double PrimeReserve { get; set; }

        [ProtoMember(8)]
        public double HashReserve { get; set; }

        [ProtoMember(9)]
        public double PrimeValue { get; set; }

        [ProtoMember(10)]
        public double HashValue { get; set; }

        [ProtoMember(11)]
        public int PooledTx { get; set; }

        [ProtoMember(12)]
        public long PrimesPerSecond { get; set; }

        [ProtoMember(13)]
        public long HashPerSecond { get; set; }

        [ProtoMember(14)]
        public DateTime CreatedOn { get; set; }
    }
}