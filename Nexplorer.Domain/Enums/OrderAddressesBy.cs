using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Nexplorer.Domain.Enums
{
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