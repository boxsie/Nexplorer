using Nexplorer.Domain.Entity.Blockchain;
using Nexplorer.Domain.Enums;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class TransactionInputOutputDto
    {
        [ProtoMember(1)]
        public int AddressId { get; set; }

        [ProtoMember(2)]
        public string AddressHash { get; set; }

        [ProtoMember(3)]
        public double Amount { get; set; }

        [ProtoMember(4)]
        public TransactionInputOutputType TransactionInputOutputType { get; set; }

        public TransactionInputOutputDto() { }

        public TransactionInputOutputDto(TransactionInputOutput txIo)
        {
            AddressId = txIo.AddressId;
            AddressHash = txIo.Address?.Hash ?? null;
            Amount = txIo.Amount;
            TransactionInputOutputType = txIo.TransactionInputOutputType;
        }
    }
}