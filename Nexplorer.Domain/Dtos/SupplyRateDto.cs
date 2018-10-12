using System;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class SupplyRateDto
    {
        [ProtoMember(1)]
        public int ChainAge { get; set; }

        [ProtoMember(2)]
        public double MoneySupply { get; set; }

        [ProtoMember(3)]
        public double TargetSupply { get; set; }

        [ProtoMember(4)]
        public double Inflationrate { get; set; }

        [ProtoMember(5)]
        public double MinuteSupply { get; set; }

        [ProtoMember(6)]
        public double HourSupply { get; set; }

        [ProtoMember(7)]
        public double DaySupply { get; set; }

        [ProtoMember(8)]
        public double WeekSupply { get; set; }

        [ProtoMember(9)]
        public double MonthSupply { get; set; }

        [ProtoMember(10)]
        public double YearSupply { get; set; }
        
        [ProtoMember(11)]
        public DateTime CreatedOn { get; set; }
    }
}