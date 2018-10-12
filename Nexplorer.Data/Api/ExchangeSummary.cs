using System;

namespace Nexplorer.Data.Api
{
    public class ExchangeSummary
    {
        public string Exchange { get; set; }
        public double Volume { get; set; }
        public double BaseVolume { get; set; }
        public double Last { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public int OpenBuyOrders { get; set; }
        public int OpenSellOrders { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
