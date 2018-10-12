using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProtoBuf;

namespace Nexplorer.Domain.Entity.Blockchain
{
    [Table("TransactionOutput")]
    public class TransactionOutput : TransactionInputOutput
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionOutputId { get; set; }
    }
}