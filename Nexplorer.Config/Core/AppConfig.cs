namespace Nexplorer.Config.Core
{
    public class AppConfig
    {
        public int BulkSaveCount { get; set; }
        public int BlockRefreshHeight { get; set; }
        public int MaxConfirmations { get; set; }
        public int BlockScanDepth { get; set; }
        public string UserSettingsCookieKey { get; set; }
    }
}