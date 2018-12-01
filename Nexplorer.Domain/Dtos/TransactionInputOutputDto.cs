using System;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Domain.Dtos
{
    public class TransactionAddressDto
    {
        public TransactionType TransactionType { get; set; }
        public string AddressHash { get; set; }
    }

    public class TransactionInputOutputDto
    {
        public TransactionType TransactionType { get; set; }
        public int BlockHeight { get; set; }
        public double Amount { get; set; }
        public int AddressId { get; set; }
    }
}