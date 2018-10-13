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
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AddressId { get; set; }

        [Required]
        public int LastBlockHeight { get; set; }

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

        [ForeignKey("AddressId")]
        public Address Address { get; set; }

        [ForeignKey("LastBlockHeight")]
        public Block LastBlock { get; set; }

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
            
            if (block.Height > LastBlockHeight)
                LastBlockHeight = block.Height;

            UpdatedOn = DateTime.Now;
        }
    }
}