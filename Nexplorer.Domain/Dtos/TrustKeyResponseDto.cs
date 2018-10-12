using System;

namespace Nexplorer.Domain.Dtos
{
    public class TrustKeyResponseDto
    {
        public string AddressHash { get; set; }
        public string TransactionHash { get; set; }
        public string GenesisBlockHash { get; set; }
        public string TrustKey { get; set; }
        public string TrustHash { get; set; }
        public double InterestRate { get; set; }
        public DateTime TimeUtc { get; set; }
        public int TrustKeyAge { get; set; }
        public int TimeSinceLastBlock { get; set; }
        public bool Expired { get; set; }
    }
}