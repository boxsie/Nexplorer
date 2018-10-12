using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Api;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Web.Controllers
{
    [Route("api/v1/")]
    [Produces("application/json")]
    public class NexplorerApiController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ExchangeQuery _exchangeQuery;
        private readonly StatQuery _statQuery;
        private readonly AddressQuery _addressQuery;

        private const int MaxPageSize = 250;
        private const int MaxAddressesReults = 1000;

        public NexplorerApiController(IMapper mapper, RedisCommand redis, ExchangeQuery exchangeQuery, StatQuery statQuery, AddressQuery addressQuery)
        {
            _mapper = mapper;
            _exchangeQuery = exchangeQuery;
            _statQuery = statQuery;
            _addressQuery = addressQuery;
        }

        // GET channel/stats
        /// <summary>
        /// Get Channel Stats
        /// </summary>
        /// <remarks>This returns the latest Nexus channel stats.</remarks>
        [HttpGet]
        [Route("channel/stats")]
        [ProducesResponseType(typeof(ChannelStats), 200)]
        public async Task<IActionResult> GetChannelStats()
        {
            var channelStats = await _statQuery.GetChannelStatsAsync();

            if (channelStats == null)
                return NotFound("There were no stats found");

            return Ok(new ChannelStats
            {
                TotalHeight = channelStats.Sum(x => x.Height),
                Channels = channelStats
            });
        }

        // GET exchange/latest
        /// <summary>
        /// Get Exchange Latest
        /// </summary>
        /// <remarks>This returns the latest Nexus price from exchanges.</remarks>
        [HttpGet]
        [Route("exchange/latest")]
        [ProducesResponseType(typeof(ExchangeSummary), 200)]
        public async Task<IActionResult> GetExchangeLatest()
        {
            var channelStats = await _exchangeQuery.GetLatestBittrexSummaryAsync();

            return Ok(new ExchangeSummary
            {
                Ask = channelStats.Ask,
                BaseVolume = channelStats.BaseVolume,
                Bid = channelStats.Bid,
                Exchange = "Bittrex",
                Last = channelStats.Last,
                OpenBuyOrders = channelStats.OpenBuyOrders,
                OpenSellOrders = channelStats.OpenSellOrders,
                TimeStamp = channelStats.TimeStamp,
                Volume = channelStats.Volume
            });
        }



        // GET addresses
        /// <summary>
        ///     Get a filtered collection of addresses
        /// </summary>
        /// <remarks>
        ///     This returns a collection of address which can be filtered.
        ///     All filter parameters are optional.
        ///     The maximum filtered results is 1000.
        ///     The maxium response size is 250.
        ///     The entire filtered results set can be obtained with multiple calls using the 'StartAt' and 'Count' parameters.
        /// </remarks>
        /// <param name="addressesCriteria">Moop</param>
        /// <returns></returns>
        [HttpGet]
        [Route("addresses")]
        [ProducesResponseType(typeof(FilterResponse<Address, AddressesCriteria>), 200)]
        public async Task<IActionResult> GetFilteredAddresses(AddressesCriteria addressesCriteria)
        {
            if (addressesCriteria == null)
                addressesCriteria = new AddressesCriteria();

            if (!addressesCriteria.StartAt.HasValue)
                addressesCriteria.StartAt = 0;

            if (!addressesCriteria.Count.HasValue)
                addressesCriteria.Count = MaxPageSize;

            var criteria = _mapper.Map<AddressFilterCriteria>(addressesCriteria);

            var addressResult = await _addressQuery.GetAddressLitesFilteredAsync(criteria, 
                addressesCriteria.StartAt.Value, addressesCriteria.Count.Value, true, MaxAddressesReults);

            var response = new FilterResponse<Address, AddressesCriteria>
            {
                Criteria = addressesCriteria,
                FilterResultCount = addressResult.ResultCount,
                Data = addressResult.Addresses.Select(x => _mapper.Map<Address>(x)).ToList()
            };

            return Ok(response);
        }
    }
}
