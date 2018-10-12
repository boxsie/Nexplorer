using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Domain.Entity.User
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public Currency Currency { get; set; }
        
        [Required]
        public DateTime RegisteredOn { get; set; }

        public virtual ICollection<FavouriteAddress> FavouriteAddresses { get; set; }
    }
}
