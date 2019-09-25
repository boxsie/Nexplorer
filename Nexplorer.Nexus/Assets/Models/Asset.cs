using System;

namespace Nexplorer.Nexus.Assets.Models
{
    public class Asset
    {
        public string Genesis { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public string TxId { get; set; }
        public string Data { get; set; }
        public DateTime CreatedOn { get; set; }

        public (string, string) GetKeyVal(string addressKey = nameof(Address), string nameKey = nameof(Name))
        {
            var useAddress = !string.IsNullOrWhiteSpace(Address);
            var key = useAddress ? addressKey.ToLower() : nameKey.ToLower();
            var val = useAddress ? Address : Name;

            return (key, val);
        }
    }
}