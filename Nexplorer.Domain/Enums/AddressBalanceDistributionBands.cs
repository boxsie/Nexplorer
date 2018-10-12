using System.ComponentModel.DataAnnotations;

namespace Nexplorer.Domain.Enums
{
    public enum AddressBalanceDistributionBands
    {
        [Display(Name = ">0-10")]
        OverZeroToTen,

        [Display(Name = "10-100")]
        OverTenToOneHundred,
        
        [Display(Name = "100-1k")]
        OverOneHundredToOneThousand,

        [Display(Name = "1k-10k")]
        OverOneThousandToTenThousand,

        [Display(Name = "10k-100k")]
        OverTenThousandToOneHundredThousand,

        [Display(Name = "100k-1m")]
        OverOneHundredThousandToOneMillion,

        [Display(Name = ">1m")]
        OneMillionPlus,
    }
}