using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Cache.Block;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Web.Hubs
{
    public class HomeMessenger
    {
        private readonly RedisCommand _redisCommand;
        private readonly BlockCacheService _blockCache;
        private readonly IHubContext<HomeHub> _homeContext;

        public HomeMessenger(RedisCommand redisCommand, BlockCacheService blockCache, IHubContext<HomeHub> homeContext)
        {
            _redisCommand = redisCommand;
            _homeContext = homeContext;
            _blockCache = blockCache;

            _redisCommand.Subscribe<BlockLiteDto>(Settings.Redis.NewBlockPubSub, SendLatestBlockDataAsync);
            _redisCommand.Subscribe<TransactionLiteDto>(Settings.Redis.NewTransactionPubSub, OnNewTransactionAsync);
            _redisCommand.Subscribe<BittrexSummaryDto>(Settings.Redis.BittrexSummaryPubSub, OnNewSummaryAsync);
        }

        private async Task SendLatestBlockDataAsync(BlockLiteDto block)
        {
            var miningInfo = await _redisCommand.GetAsync<MiningInfoDto>(Settings.Redis.MiningInfoLatest);

            var difficulty = new
            {
                Pos = block.Channel == "PoS"
                    ? block.Difficulty
                    : await _blockCache.GetChannelDifficultyAsync(BlockChannels.PoS),
                Hash = miningInfo.HashDifficulty,
                Prime = miningInfo.PrimeDifficulty
            };

            var blockDataJson = Helpers.JsonSerialise(new
            {
                Difficulty = difficulty,
                Block = block
            });

            await _homeContext.Clients.All.SendAsync("NewBlockPubSub", blockDataJson);
        }

        private Task OnNewTransactionAsync(TransactionLiteDto tx)
        {
            return _homeContext.Clients.All.SendAsync("NewTxPubSub", Helpers.JsonSerialise(tx));
        }

        private Task OnNewSummaryAsync(BittrexSummaryDto summary)
        {
            return _homeContext.Clients.All.SendAsync("NewBittrexSummaryPubSub", Helpers.JsonSerialise(summary));
        }
    }
}