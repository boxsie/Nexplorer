namespace Nexplorer.Config.Core
{
    public class AppConfig
    {
        public int BulkSaveCount { get; set; }
        public int BlockRefreshHeight { get; set; }
        public int MaxConfirmations { get; set; }
        public int BlockScanDepthLong { get; set; }
        public int BlockScanDepthShort { get; set; }
        public string UserSettingsCookieKey { get; set; }
    }
}