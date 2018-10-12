using System;
using System.Collections.Generic;
using System.Text;
using Nexplorer.Domain.Entity;
using Nexplorer.Domain.Entity.Exchange;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class BittrexSummaryDto
    {
        [ProtoMember(1)]
        public double Volume { get; set; }

        [ProtoMember(2)]
        public double BaseVolume { get; set; }

        [ProtoMember(3)]
        public double Last { get; set; }

        [ProtoMember(4)]
        public double Bid { get; set; }

        [ProtoMember(5)]
        public double Ask { get; set; }

        [ProtoMember(6)]
        public int OpenBuyOrders { get; set; }

        [ProtoMember(7)]
        public int OpenSellOrders { get; set; }

        [ProtoMember(8)]
        public DateTime TimeStamp { get; set; }

        public BittrexSummaryDto() { }

        public BittrexSummaryDto(BittrexSummary summary)
        {
            Volume = summary.Volume;
            BaseVolume = summary.BaseVolume;
            Last = summary.Last;
            Bid = summary.Bid;
            Ask = summary.Ask;
            OpenBuyOrders = summary.OpenBuyOrders;
            OpenSellOrders = summary.OpenSellOrders;
            TimeStamp = summary.TimeStamp;
        }
    }
}
