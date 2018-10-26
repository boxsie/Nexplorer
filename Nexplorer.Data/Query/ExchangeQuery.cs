using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Domain.Dtos;

namespace Nexplorer.Data.Query
{
    public class ExchangeQuery
    {
        private readonly RedisCommand _redis;

        public ExchangeQuery(RedisCommand redis)
        {
            _redis = redis;
        }

        public Task LatestBittrexSummarySubscribe(Func<BittrexSummaryDto, Task> onPublish)
        {
            var redisKey = Settings.Redis.BittrexSummaryPubSub;

            _redis.Subscribe(redisKey, onPublish);

            return Task.CompletedTask;
        }

        public async Task<BittrexSummaryDto> GetLatestBittrexSummaryAsync()
        {
            return await _redis.GetAsync<BittrexSummaryDto>(Settings.Redis.BittrexSummaryPubSub);
        }

        //public async Task<BittrexSummaryDto> GetLatestBittrexSummaryAsync()
        //{
        //    const string sqlQ = @"SELECT TOP 1 Volume, BaseVolume, Last, Bid, Ask, OpenBuyOrders, OpenSellOrders, TimeStamp FROM BittrexSummary ORDER BY BittrexSummaryId DESC;";

        //    using (var sqlCon = await DbConnectionFactory.GetNexusDbConnectionAsync())
        //    {
        //        var result = (await sqlCon.QueryAsync<BittrexSummaryDto>(sqlQ)).FirstOrDefault();

        //        return result;
        //    }
        //}
    }
}