using System;
using System.Globalization;

namespace Nexplorer.Domain.Models
{
    public class AddressTransactionModel
    {
        public string TransactionHash { get; set; }
        public double Amount { get; set; }
        public string AmountText { get; set; }
        public string Date { get; set; }

        public AddressTransactionModel(string hash, double amount, DateTime time)
        {
            TransactionHash = hash;
            Amount = amount;
            AmountText = amount.ToString("N6", CultureInfo.InvariantCulture);
            Date = time.ToShortDateString();
        }
    }
}