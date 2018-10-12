using System.Collections.Generic;

namespace Nexplorer.Domain.Dtos
{
    public class AddressFilterResult
    {
        public int ResultCount { get; set; }
        public List<AddressLiteDto> Addresses { get; set; }
    }
}