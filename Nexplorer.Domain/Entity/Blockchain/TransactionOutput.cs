using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProtoBuf;

namespace Nexplorer.Domain.Entity.Blockchain
{
    [Table("TransactionOutput")]
    public class TransactionOutput
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionOutputId { get; set; }

        [Required]
        public int TransactionId { get; set; }

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