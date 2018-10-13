using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexplorer.Domain.Entity.Orphan
{
    [Table("OrphanTransaction")]
    public class OrphanTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }
        
        [Required]
        public int BlockHeight { get; set; }
        
        [Required]
        [MaxLength(256)]
        public string Hash { get; set; }

        [ForeignKey("BlockHeight")]
        public OrphanBlock OrphanBlock { get; set; }
    }
}