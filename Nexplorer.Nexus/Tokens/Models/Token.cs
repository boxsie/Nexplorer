namespace Nexplorer.Nexus.Tokens.Models
{
    public abstract class Token
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Identifier { get; set; }
        public string Tx { get; set; }

        public abstract string Type { get; }
        public abstract int TypeId { get; }

        public (string, string) GetKeyVal(string addressKey = nameof(Address), string nameKey = nameof(Name))
        {
            var useAddress = !string.IsNullOrWhiteSpace(Address);
            var key = useAddress ? addressKey.ToLower() : nameKey.ToLower();
            var val = useAddress ? Address : Name;

            return (key, val);
        }
    }
}