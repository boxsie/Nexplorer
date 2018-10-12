using System;
using Nexplorer.Domain.Enums;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class AddressTransactionDto
    {
        [ProtoMember(1)]
        public TransactionType TxType { get; set; }

        [ProtoMember(2)]
        public int BlockHeight { get; set; }

        [ProtoMember(3)]
        public string TransactionHash { get; set; }

        [ProtoMember(4)]
        public double Amount { get; set; }

        [ProtoMember(5)]
        public DateTime TimeUtc { get; set; }
    }
}