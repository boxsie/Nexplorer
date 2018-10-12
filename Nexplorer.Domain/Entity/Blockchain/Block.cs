using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProtoBuf;

namespace Nexplorer.Domain.Entity.Blockchain
{
    [Table("Block")]
    public class Block
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Height { get; set; }
        
        [Required]
        [MaxLength(256)]
        public string Hash { get; set; }
        
        public int Size { get; set; }
        public int Channel { get; set; }
        public int Version { get; set; }
        public string MerkleRoot { get; set; }
        public DateTime TimeUtc { get; set; }
        public double Nonce { get; set; }
        public string Bits { get; set; }
        public double Difficulty { get; set; }
        public double Mint { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
