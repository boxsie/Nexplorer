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

        [Required]
        public int Size { get; set; }

        [Required]
        public int Channel { get; set; }

        [Required]
        public int Version { get; set; }

        [Required]
        [MaxLength(256)]
        public string MerkleRoot { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public double Nonce { get; set; }

        [Required]
        [MaxLength(256)]
        public string Bits { get; set; }

        [Required]
        public double Difficulty { get; set; }

        [Required]
        public double Mint { get; set; }
        
        public List<Transaction> Transactions { get; set; }
    }
}
