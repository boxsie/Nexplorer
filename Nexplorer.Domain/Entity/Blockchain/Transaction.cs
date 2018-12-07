using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nexplorer.Domain.Enums;
using ProtoBuf;

namespace Nexplorer.Domain.Entity.Blockchain
{
    [Table("Transaction")]
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }
        
        [Required]
        public int BlockHeight { get; set; }
        
        [Required]
        [MaxLength(256)]
        public string Hash { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public double Amount { get; set; }

        public TransactionType? TransactionType { get; set; }

        [ForeignKey("BlockHeight")]
        public Block Block { get; set; }

        public List<TransactionInputOutput> InputOutputs { get; set; }
    }
}
