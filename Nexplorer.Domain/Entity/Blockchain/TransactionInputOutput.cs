using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexplorer.Domain.Entity.Blockchain
{
    public abstract class TransactionInputOutput
    {
        [Required]
        public virtual Transaction Transaction { get; set; }
        
        [Required]
        public virtual Address Address { get; set; }
        
        [Required]
        public double Amount { get; set; }
    }
}