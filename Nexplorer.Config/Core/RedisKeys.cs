namespace Nexplorer.Config.Core
{
    public class RedisKeys
    {
        public string CachedHeight { get; set; }
        public string SyncedHeight { get; set; }

        // Cache
        public string BlockCache { get; set; }
        public string BlockLiteCache { get; set; }
        public string TransactionLiteCache { get; set; }
        public string AddressCache { get; set; }
        public string TrustKeyCache { get; set; }
        public string TrustKeyAddressCache { get; set; }
        public string NexusAddressCache { get; set; }
        public string BlockSyncStreamCache { get; set; }
        public string BlockSyncStreamCacheHeight { get; set; }

        // Stats
        public string BlockCount24Hours { get; set; }
        public string TransactionCount24Hours { get; set; }
        public string TimestampUtcLatest { get; set; }
        public string PeerInfoLatest { get; set; }
        public string MiningInfoLatest { get; set; }
        public string MiningInfo10Mins { get; set; }
        public string SupplyRatesLatest { get; set; }
        public string ChannelStatsLatest { get; set; }
        public string BittrexLastUsdBtcPrice { get; set; }
        public string BittrexLastBtcNxsPrice { get; set; }
        public string AddressDistributionStats { get; set; }

        // Pub Sub
        public string NewBlockPubSub { get; set; }
        public string NewTransactionPubSub { get; set; }
        public string BittrexSummaryPubSub { get; set; }
        public string MiningStatPubSub { get; set; }
        public string DifficultyStatPubSub { get; set; }
        public string AddressStatPubSub { get; set; }

        public string NodeVersion { get; set; }
        public string SyncOutputPubSub { get; set; }

        public string BuildCachedBlockKey(int height)
        {
            return $"{BlockCache}:{height}";
        }

        public string BuildCachedAddressKey(string hash)
        {
            return $"{AddressCache}:{hash}";
        }
    }
}