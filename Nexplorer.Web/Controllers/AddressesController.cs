using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Criteria;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Entity.User;
using Nexplorer.Domain.Enums;
using Nexplorer.Web.Dtos;
using Nexplorer.Web.Enums;
using Nexplorer.Web.Extensions;
using Nexplorer.Web.Models;
using Nexplorer.Web.Queries;

namespace Nexplorer.Web.Controllers
{
    public class AddressesController : WebControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AddressQuery _addressQuery;
        private readonly CurrencyQuery _currencyQuery;
        private readonly ExchangeQuery _exchangeQuery;
        private readonly BlockQuery _blockQuery;
        private readonly UserQuery _userQuery;
        private readonly TransactionQuery _transactionQuery;

        private const int TransactionsPerPage = 5;
        private const int MaxAddressesPerFilterPage = 100;
        private const int MaxAddressesFilterResults = 1000;

        public AddressesController(UserManager<ApplicationUser> userManager, AddressQuery addressQuery, CurrencyQuery currencyQuery,
            ExchangeQuery exchangeQuery, BlockQuery blockQuery, UserQuery userQuery, TransactionQuery transactionQuery)
        {
            _userManager = userManager;
            _addressQuery = addressQuery;
            _currencyQuery = currencyQuery;
            _exchangeQuery = exchangeQuery;
            _blockQuery = blockQuery;
            _userQuery = userQuery;
            _transactionQuery = transactionQuery;
        }
        
        public async Task<IActionResult> Index()
        {
            var viewModel = new AddressIndexViewModel
            {
                AddressDistrubtion = await _addressQuery.GetAddressesDistributionBandsAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Address(string addressHash)
        {
            if (addressHash == null)
                return RedirectToAction("index");
            
            var addressDto = await _addressQuery.GetAddressAsync(addressHash);

            if (addressDto == null)
                return RedirectToAction("index");

            var txSentFloor = (int)Math.Floor((double)addressDto.SentCount / TransactionsPerPage);
            var txRecFloor = (int)Math.Floor((double)addressDto.ReceivedCount / TransactionsPerPage);
            var txSentRecFloor = (int)Math.Floor(((double)addressDto.SentCount + addressDto.ReceivedCount) / TransactionsPerPage);

            var user = await _userManager.GetUserAsync(User);

            var faveAddress = (await _userQuery.GetFavouriteAddressesAsync(user?.Id))?
                .FirstOrDefault(x => x.AddressId == addressDto.AddressId);

            var currency = user?.Currency ?? Currency.USD;

            var nxsCurrency = new NxsCurrencyHelper(
                currency,
                addressDto.Balance,
                await _currencyQuery.GetLatestNXSPriceInBTCAsync(),
                await _currencyQuery.GetLatestBTCPriceInUSDAsync(),
                (double) await _currencyQuery.ConvertFromUSDAsync(currency));

            var viewModel = new AddressViewModel
            {
                Address = addressDto,
                TrustKey = await _addressQuery.GetAddressTrustKey(addressDto.Hash),
                TxPerPage = TransactionsPerPage,
                TxSentPageCount = addressDto.SentCount % TransactionsPerPage == 0 
                    ? txSentFloor 
                    : txSentFloor + 1,
                TxReceivedPageCount = addressDto.ReceivedCount % TransactionsPerPage == 0 
                    ? txRecFloor 
                    : txRecFloor + 1,
                TxSentReceivedPageCount = (addressDto.SentCount + addressDto.ReceivedCount) % TransactionsPerPage == 0 
                    ? txSentRecFloor 
                    : txSentRecFloor + 1,
                NxsCurrency = nxsCurrency,
                LastBlockSeenTimestamp = await _blockQuery.GetBlockTimestamp(addressDto.LastBlockSeen),
                IsUserFavourite = faveAddress != null,
                AddressAlias = faveAddress?.Alias
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetAddressBalance(string addressHash, int days)
        {
            if (days == 0 || days > 180)
                return BadRequest($"Amount of days ({days}) is not valid. It must be greater than 0 and less than 28");

            return Ok(await _addressQuery.GetAddressBalance(addressHash, days));
        }

        [HttpPost]
        public async Task<IActionResult> GetAddressTxs(DataTablePostModel<AddressTransactionFilterCriteria> model)
        {
            var count = model.Length > MaxAddressesPerFilterPage
                ? MaxAddressesPerFilterPage
                : model.Length;

            if (model.FilterCriteria == null || !model.FilterCriteria.AddressHashes.Any())
                return NotFound("Address hash is required!");

            var filterCriteria = model.FilterCriteria ?? new AddressTransactionFilterCriteria();

            var txResult = await _addressQuery.GetAddressTransactionsFilteredAsync(filterCriteria, model.Start, count, true);
            
            var response = new
            {
                Draw = model.Draw,
                RecordsTotal = 0,
                RecordsFiltered = txResult.ResultCount,
                Data = txResult.Results
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> GetAddresses(DataTablePostModel<AddressFilterCriteria> model)
        {
            var criteria = model.Filter != "custom"
                ? GetCriteria(model.Filter)
                : model.FilterCriteria;

            var count = model.Length > MaxAddressesPerFilterPage
                ? MaxAddressesPerFilterPage
                : model.Length;

            var countable = model.Filter == "staking" || model.Filter == "nexus" || model.Filter == "custom";

            var data = await _addressQuery.GetAddressLitesFilteredAsync(criteria, model.Start, count, countable, MaxAddressesFilterResults);

            var resultCount = countable ? data.ResultCount : MaxAddressesFilterResults;

            var response = new
            {
                Draw = model.Draw,
                RecordsTotal = 0,
                RecordsFiltered = resultCount,
                Data = data.Results
            };

            return Ok(response);
        }

        private AddressFilterCriteria GetCriteria(string filter)
        {
            switch (filter)
            {
                case "recent":
                    return new AddressFilterCriteria { OrderBy = OrderAddressesBy.MostRecentlyActive };
                case "staking":
                    return new AddressFilterCriteria { OrderBy = OrderAddressesBy.HighestInterestRate, IsStaking = true };
                case "nexus":
                    return new AddressFilterCriteria { OrderBy = OrderAddressesBy.HighestBalance, IsNexus = true };
                default:
                    return new AddressFilterCriteria { OrderBy = OrderAddressesBy.HighestBalance };
            }
        }
    }
}
