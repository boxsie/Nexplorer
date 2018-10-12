using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProtoBuf;

namespace Nexplorer.Domain.Entity.Blockchain
{
    [Table("TransactionInput")]
    public class TransactionInput : TransactionInputOutput
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionInputId { get; set; }
    }
}