using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public virtual Block Block { get; set; }
        
        [Required]
        [MaxLength(256)]
        public string Hash { get; set; }
        
        public int Confirmations { get; set; }
        public DateTime TimeUtc { get; set; }
        public double Amount { get; set; }
        public virtual ICollection<TransactionInput> Inputs { get; set; }
        public virtual ICollection<TransactionOutput> Outputs { get; set; }
    }
}
