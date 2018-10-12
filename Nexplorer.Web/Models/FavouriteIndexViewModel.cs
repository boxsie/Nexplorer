using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.User;

namespace Nexplorer.Web.Models
{
    public class FavouriteIndexViewModel
    {
        public List<FavouriteAddressDto> FavouriteAddresses { get; set; }
    }
}