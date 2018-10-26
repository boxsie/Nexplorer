using System.ComponentModel.DataAnnotations;

namespace Nexplorer.Domain.Enums
{
    public enum OrderTransactionsBy
    {
        [Display(Name = "Highest Amount")]
        HighestAmount,

        [Display(Name = "Lowest Amount")]
        LowestAmount,

        [Display(Name = "Most Recent")]
        MostRecent,

        [Display(Name = "Least Recent")]
        LeastRecent
    }
}