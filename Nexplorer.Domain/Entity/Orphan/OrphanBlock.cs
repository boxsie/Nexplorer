using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexplorer.Domain.Entity.Orphan
{
    [Table("OrphanBlock")]
    public class OrphanBlock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlockId { get; set; }

        [Required]
        public int Height { get; set; }
        
        [Required]
        [MaxLength(256)]
        public string Hash { get; set; }
        
        public DateTime TimeUtc { get; set; }
        
        public virtual ICollection<OrphanTransaction> Transactions { get; set; }
    }
}