using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexplorer.Domain.Entity.Blockchain
{
    [Table("TrustKey")]
    public class TrustKey
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TrustKeyId { get; set; }

        [Required]
        public int GenesisHeight { get; set; }

        [Required]
        public int AddressId { get; set; }

        [Required]
        public int TransactionId { get; set; }

        [Required]
        public string Key { get; set; }

        [Required]
        public string Hash { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }
        
        [ForeignKey("GenesisHeight")]
        public Block GenesisBlock { get; set; }

        [ForeignKey("AddressId")]
        public Address Address { get; set; }

        [ForeignKey("TransactionId")]
        public Transaction Transaction { get; set; }
    }
}