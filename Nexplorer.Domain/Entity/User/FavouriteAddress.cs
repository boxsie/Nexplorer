using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexplorer.Domain.Entity.User
{
    [Table("FavouriteAddress")]
    public class FavouriteAddress
    {
        public const int AliasMaxLength = 100;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FavouriteAddressId { get; set; }

        [Required]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int AddressId { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [MaxLength(AliasMaxLength)]
        public string Alias { get; set; }
        
        public FavouriteAddress()
        {
            CreatedOn = DateTime.Now;
        }
    }
}
