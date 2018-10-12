namespace Nexplorer.Data.Cache.Block
{
    public class BlockCacheTransaction
    {
        public int Height { get; set; }
        public string TxHash { get; set; }
        public BlockCacheTransactionUpdate TransactionUpdate { get; set; }
    }
}