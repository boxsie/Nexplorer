using System;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Domain.Dtos
{
    public class TransactionInputOutputDto
    {
        public TransactionType TxType { get; set; }
        public int BlockHeight { get; set; }
        public double Amount { get; set; }
        public int AddressId { get; set; }
    }
}