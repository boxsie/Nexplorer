using System;
using Nexplorer.Domain.Entity.Blockchain;

namespace Nexplorer.Domain.Dtos
{
    public class AddressDto
    {
        public int AddressId { get; set; }
        public string Hash { get; set; }
        public int ReceivedCount { get; set; }
        public int SentCount { get; set; }
        public double ReceivedAmount { get; set; }
        public double SentAmount { get; set; }
        public int FirstBlockSeen { get; set; }
        public int LastBlockSeen { get; set; }

        public double Balance => Math.Round(ReceivedAmount - SentAmount, 8);
    }
}