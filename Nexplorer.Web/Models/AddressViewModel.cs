using System;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;
using Nexplorer.Domain.Models;

namespace Nexplorer.Web.Models
{
    public class AddressViewModel
    {
        public AddressDto Address { get; set; }
        public TrustKeyDto TrustKey { get; set; }

        public int TxPerPage { get; set; }
        public int TxSentPageCount { get; set; }
        public int TxReceivedPageCount { get; set; }
        public int TxSentReceivedPageCount { get; set; }

        public DateTime LastBlockSeenTimestamp { get; set; }

        public NxsCurrencyHelper NxsCurrency { get; set; }
        public bool IsUserFavourite { get; set; }
        public string AddressAlias { get; set; }
    }

    public class NxsCurrencyHelper
    {
        public Currency Currency { get; }
        public double NXSValue { get; }
        public double BTCValue { get; }

        public NxsCurrencyHelper(Currency currency, double nxsBalance, double nxsToBtc, double btcToUsd, double currencyFromUsd)
        {
            Currency = currency;

            BTCValue = Math.Round(nxsBalance * nxsToBtc, 8);

            var nxsInUsd = BTCValue * btcToUsd;

            if (currency == Currency.USD)
                NXSValue = Math.Round(nxsInUsd, 8);
            else
                NXSValue = Math.Round(nxsInUsd * currencyFromUsd, 8);
        }
    }
}