using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Nexplorer.Domain.Enums
{
    public enum NexusAddressPools
    {
        [Display(Name = "USA Embassy")]
        USA,
        [Display(Name = "UK Embassy")]
        UK,
        [Display(Name = "Australia Embassy")]
        AUS,
        [Display(Name = "Community")]
        Community,
        [Display(Name = "Retired")]
        Retired
    }

    public enum OrderAddressesBy
    {
        [Display(Name = "Highest Balance")]
        HighestBalance,

        [Display(Name = "Lowest Balance")]
        LowestBalance,

        [Display(Name = "Most Recent")]
        MostRecentlyActive,

        [Display(Name = "Least Recent")]
        LeastRecentlyActive,

        [Display(Name = "Highest Interest Rate")]
        HighestInterestRate,

        [Display(Name = "Lowest Interest Rate")]
        LowestInterestRate
    }
}