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
        public virtual Address Address { get; set; }

        [Required]
        public virtual Transaction Transaction { get; set; }

        [Required]
        public virtual Block GenesisBlock { get; set; }

        [Required]
        public string Key { get; set; }

        [Required]
        public string Hash { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }
    }
}