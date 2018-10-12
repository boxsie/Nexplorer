using System;

namespace Nexplorer.Domain.Dtos
{
    public class FavouriteAddressDto
    {
        public int FavouriteAddressId { get; set; }
        public int AddressId { get; set; }
        public string Alias { get; set; }
        public DateTime CreatedOn { get; set; }

        public AddressLiteDto AddressDto { get; set; }
    }
}