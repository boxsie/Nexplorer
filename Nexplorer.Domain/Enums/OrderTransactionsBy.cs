using System.ComponentModel.DataAnnotations;

namespace Nexplorer.Domain.Enums
{
    public enum OrderBlocksBy
    {
        [Display(Name = "Highest")]
        Highest,

        [Display(Name = "Lowest")]
        Lowest,

        [Display(Name = "Largest Size")]
        Largest,

        [Display(Name = "Smallest Size")]
        Smallest
    }

    public enum OrderTransactionsBy
    {
        [Display(Name = "Most Recent")]
        MostRecent,

        [Display(Name = "Least Recent")]
        LeastRecent,

        [Display(Name = "Highest Amount")]
        HighestAmount,

        [Display(Name = "Lowest Amount")]
        LowestAmount
    }
}