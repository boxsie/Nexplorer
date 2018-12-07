using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nexplorer.Domain.Enums;
using ProtoBuf;

namespace Nexplorer.Domain.Entity.Blockchain
{
    [Table("TransactionInputOutput")]
    public class TransactionInputOutput
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionInputOutputId { get; set; }

        [Required]
        public int TransactionId { get; set; }

        [Required]
        public TransactionInputOutputType TransactionInputOutputType { get; set; }

        [Required]
        public int AddressId { get; set; }

        [Required]
        public double Amount { get; set; }

        [ForeignKey("TransactionId")]
        public Transaction Transaction { get; set; }

        [ForeignKey("AddressId")]
        public Address Address { get; set; }
    }
}