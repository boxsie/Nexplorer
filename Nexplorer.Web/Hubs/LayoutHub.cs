using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Query;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Web.Hubs
{
    public class LayoutHub : Hub
    {
        private readonly RedisCommand _redisCommand;
        private readonly BlockQuery _blockQuery;
        private readonly ExchangeQuery _exchangeQuery;

        public LayoutHub(RedisCommand redisCommand, BlockQuery blockQuery, ExchangeQuery exchangeQuery)
        {
            _redisCommand = redisCommand;
            _blockQuery = blockQuery;
            _exchangeQuery = exchangeQuery;
        }

        public Task<DateTime> GetLatestTimestampUtc()
        {
            return _redisCommand.GetAsync<DateTime>(Settings.Redis.TimestampUtcLatest);
        }

        public Task<BlockDto> GetLatestBlock()
        {
            return _blockQuery.GetLastBlockAsync();
        }

        public Task<BittrexSummaryDto> GetLatestPrice()
        {
            return _exchangeQuery.GetLatestBittrexSummaryAsync();
        }

        public Task<Dictionary<string, double>> GetLatestDiffs()
        {
            return _redisCommand.GetAsync<Dictionary<string, double>>(Settings.Redis.DifficultyStatPubSub);
        }
    }
}