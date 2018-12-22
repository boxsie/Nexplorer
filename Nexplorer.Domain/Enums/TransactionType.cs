using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Nexplorer.Domain.Enums
{
    public enum TransactionType
    {
        [Display(Name = "Coinbase hash")]
        CoinbaseHash,

        [Display(Name = "Coinbase prime")]
        CoinbasePrime,

        [Display(Name = "Coinstake")]
        Coinstake,

        [Display(Name = "Coinstake genesis")]
        CoinstakeGenesis,

        [Display(Name = "User")]
        User
    }
}