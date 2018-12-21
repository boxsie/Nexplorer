using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Domain.Dtos;
using Nexplorer.Domain.Enums;

namespace Nexplorer.Web.Hubs
{
    public class LayoutMessenger
    {
        private readonly IHubContext<LayoutHub> _layoutContext;

        public LayoutMessenger(RedisCommand redisCommand, IHubContext<LayoutHub> layoutContext)
        {
            _layoutContext = layoutContext;

            redisCommand.Subscribe<DateTime>(Settings.Redis.TimestampUtcLatest, OnLatestTimestampUtc);
            redisCommand.Subscribe<BlockLiteDto>(Settings.Redis.NewBlockPubSub, UpdateLatestBlockData);
            redisCommand.Subscribe<BittrexSummaryDto>(Settings.Redis.BittrexSummaryPubSub, UpdateLatestPriceData);
            redisCommand.Subscribe<Dictionary<string, double>>(Settings.Redis.DifficultyStatPubSub, UpdateLatestDiffData);
        }

        private Task OnLatestTimestampUtc(DateTime timestamp)
        {
            return _layoutContext.Clients.All.SendAsync("UpdateTimestampUtc", timestamp);
        }

        private Task UpdateLatestBlockData(BlockLiteDto block)
        {
            return _layoutContext.Clients.All.SendAsync("UpdateLatestBlockData", block);
        }

        private Task UpdateLatestPriceData(BittrexSummaryDto summary)
        {
            return _layoutContext.Clients.All.SendAsync("UpdateLatestPriceData", summary);
        }

        private Task UpdateLatestDiffData(Dictionary<string, double> difficulties)
        {
            return _layoutContext.Clients.All.SendAsync("UpdateLatestDiffData", difficulties);
        }
    }
}