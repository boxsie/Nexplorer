using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Domain.Entity.Blockchain
{
    [Table("AddressAggregate")]
    public class AddressAggregate
    {
        [Key]
        public int AddressId { get; set; }

        [ForeignKey("AddressId")]
        public virtual Address Address { get; set; }

        [Required]
        public Block LastBlock { get; set; }

        [Required]
        public double Balance { get; set; }

        [Required]
        public double ReceivedAmount { get; set; }

        [Required]
        public int ReceivedCount { get; set; }

        [Required]
        public double SentAmount { get; set; }

        [Required]
        public int SentCount { get; set; }

        [Required]
        public DateTime UpdatedOn { get; set; }

        public void ModifyAggregateProperties(TransactionType txType, double amount, Block block)
        {
            switch (txType)
            {
                case TransactionType.Input:
                    SentAmount = Math.Round(SentAmount + amount, 8);
                    SentCount++;
                    break;
                case TransactionType.Output:
                    ReceivedAmount = Math.Round(ReceivedAmount + amount, 8);
                    ReceivedCount++;
                    break;
            }

            Balance = Math.Round(ReceivedAmount - SentAmount, 8);
            
            if (block.Height > (LastBlock?.Height ?? 0))
                LastBlock = block;

            UpdatedOn = DateTime.Now;
        }
    }
}