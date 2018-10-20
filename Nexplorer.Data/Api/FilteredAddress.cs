namespace Nexplorer.Data.Api
{
    public class FilteredAddress
    {
        public string Hash { get; set; }
        public double Balance { get; set; }
        public int FirstBlockHeight { get; set; }
        public int LastBlockHeight { get; set; }
        public double? InterestRate { get; set; }
        public bool IsNexus { get; set; }
    }
}