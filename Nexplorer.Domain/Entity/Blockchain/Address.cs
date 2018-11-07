using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexplorer.Domain.Entity.Blockchain
{
    [Table("Address")]
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AddressId { get; set; }

        [Required]
        [MaxLength(256)]
        public string Hash { get; set; }

        [Required]
        public int FirstBlockHeight { get; set; }
        
        //[ForeignKey("FirstBlockHeight")]
        public Block FirstBlock { get; set; }
    }
}