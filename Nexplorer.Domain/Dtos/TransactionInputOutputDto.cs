using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class TransactionInputOutputDto
    {
        [ProtoMember(1)]
        public string AddressHash { get; set; }

        [ProtoMember(2)]
        public double Amount { get; set; }
    }
}