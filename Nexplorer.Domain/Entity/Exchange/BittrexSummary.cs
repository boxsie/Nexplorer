using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexplorer.Domain.Entity.Exchange
{
    [Table("BittrexSummary")]
    public class BittrexSummary
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BittrexSummaryId { get; set; }
        
        [Required]
        [MaxLength(10)]
        public string MarketName { get; set; }

        [Required]
        public double Volume { get; set; }

        [Required]
        public double BaseVolume { get; set; }

        [Required]
        public double Last { get; set; }

        [Required]
        public double Bid { get; set; }

        [Required]
        public double Ask { get; set; }

        [Required]
        public int OpenBuyOrders { get; set; }

        [Required]
        public int OpenSellOrders { get; set; }

        [Required]
        public DateTime TimeStamp { get; set; }
    }
}
