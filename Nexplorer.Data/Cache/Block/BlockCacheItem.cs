namespace Nexplorer.Data.Cache.Block
{
    public class BlockCacheItem<T>
    {
        public bool NeedsUpdate { get; set; }
        public T Item { get; set; }

        public BlockCacheItem(T item, bool needsUpdate = false)
        {
            Item = item;
            NeedsUpdate = needsUpdate;
        }
    }
}
