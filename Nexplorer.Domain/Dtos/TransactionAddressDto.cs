using Nexplorer.Domain.Enums;

namespace Nexplorer.Domain.Dtos
{
    public class TransactionAddressDto
    {
        public TransactionInputOutputType TransactionInputOutputType { get; set; }
        public string AddressHash { get; set; }
    }
}