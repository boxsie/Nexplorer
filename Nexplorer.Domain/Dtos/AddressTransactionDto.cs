using System;
using System.Collections.Generic;
using Nexplorer.Domain.Enums;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class AddressTransactionItemDto
    {
        [ProtoMember(1)]
        public string AddressHash { get; set; }

        [ProtoMember(2)]
        public double Amount { get; set; }
    }

    [ProtoContract]
    public class AddressTransactionDto
    {
        [ProtoMember(1)]
        public string AddressHash { get; set; }

        [ProtoMember(2)]
        public TransactionInputOutputType TransactionInputOutputType { get; set; }

        [ProtoMember(3)]
        public int BlockHeight { get; set; }

        [ProtoMember(4)]
        public string TransactionHash { get; set; }

        [ProtoMember(5)]
        public double Amount { get; set; }

        [ProtoMember(6)]
        public DateTime Timestamp { get; set; }
        
        [ProtoMember(7)]
        public TransactionType TransactionType { get; set; }

        [ProtoMember(8)]
        public List<AddressTransactionItemDto> OppositeItems { get; set; }

        [ProtoMember(9)]
        public int TransactionId { get; set; }
    }
}