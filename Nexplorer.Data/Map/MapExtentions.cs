using System;
using System.Linq;
using System.Threading.Tasks;
using Nexplorer.Domain.Entity.Exchange;
using Nexplorer.Infrastructure.Bittrex.Models;

namespace Nexplorer.Data.Map
{
    public static class MapExtentions
    {
        public static BittrexSummary ToBittrexSummary(this BittrexSummaryResponse summaryResponse)
        {
            return new BittrexSummary
            {
                Ask = summaryResponse.Ask,
                Bid = summaryResponse.Bid,
                Last = summaryResponse.Last,
                MarketName = summaryResponse.MarketName,
                OpenBuyOrders = summaryResponse.OpenBuyOrders,
                OpenSellOrders = summaryResponse.OpenSellOrders,
                Volume = summaryResponse.Volume,
                BaseVolume = summaryResponse.BaseVolume,
                TimeStamp = summaryResponse.TimeStamp
            };
        }
    }
}
