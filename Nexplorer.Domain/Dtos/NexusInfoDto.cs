using System;

namespace Nexplorer.Domain.Dtos
{
    public class NexusInfoDto
    {
        public string Version { get; set; }
        
        public int ProtocolVersion { get; set; }
        
        public int WalletVersion { get; set; }
        
        public double Balance { get; set; }
        
        public double NewMint { get; set; }
        
        public double Stake { get; set; }
        
        public double InterestWeight { get; set; }
        
        public double StakeWeight { get; set; }
        
        public double TrustWeight { get; set; }
        
        public double BlockHeight { get; set; }
        
        public int Blocks { get; set; }
        
        public DateTime TimeStampUtc { get; set; }
        
        public int Connections { get; set; }
    }
}