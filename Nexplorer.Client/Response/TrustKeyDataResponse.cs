using System;

namespace Nexplorer.Client.Response
{
    public class TrustKeyDataResponse
    {
        public string TransactionHash { get; set; }
        public string GenesisBlockHash { get; set; }
        public string TrustKey { get; set; }
        public string TrustHash { get; set; }
        public DateTime TimeUtc { get; set; }
        public int TrustKeyAge { get; set; }
        public int TimeSinceLastBlock { get; set; }
        public bool Expired { get; set; }
    }
}