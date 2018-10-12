namespace Nexplorer.Config.Core
{
    public class AppSettings
    {
        public int BulkSaveCount { get; set; }
        public int BlockRefreshHeight { get; set; }
        public int MaxConfirmations { get; set; }
        public int BlockCacheCount { get; set; }
        public int BlockLiteCacheCount { get; set; }
        public int TransactionLiteCacheCount { get; set; }
    }
}