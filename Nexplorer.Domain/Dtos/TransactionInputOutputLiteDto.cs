using Nexplorer.Domain.Enums;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class TransactionInputOutputLiteDto
    {
        [ProtoMember(1)]
        public string AddressHash { get; set; }

        [ProtoMember(2)]
        public double Amount { get; set; }

        [ProtoMember(3)]
        public TransactionInputOutputType TransactionInputOutputType { get; set; }
    }
}