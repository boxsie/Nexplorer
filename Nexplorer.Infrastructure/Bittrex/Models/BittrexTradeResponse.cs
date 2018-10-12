using System;

namespace Nexplorer.Infrastructure.Bittrex.Models
{
    public class BittrexTradeResponse
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public double Total { get; set; }
        public string FillType { get; set; }
        public string OrderType { get; set; }
    }
}