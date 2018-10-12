using System.Collections.Generic;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class CachedAddressDto
    {
        [ProtoMember(1)]
        public string Hash { get; set; }

        [ProtoMember(2)]
        public int FirstBlockHeight { get; set; }

        [ProtoMember(3)]
        public AddressAggregateDto Aggregate { get; set; }

        [ProtoMember(4)]
        public List<AddressTransactionDto> AddressTransactions { get; set; }
    }
}