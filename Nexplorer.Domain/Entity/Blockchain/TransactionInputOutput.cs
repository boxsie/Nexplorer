using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexplorer.Domain.Entity.Blockchain
{
    public abstract class TransactionInputOutput
    {
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