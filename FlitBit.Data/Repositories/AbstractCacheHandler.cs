namespace FlitBit.Data.Repositories
{
	public abstract class AbstractCacheHandler<TModel>
	{
		public abstract void PerformUpdateCacheItem(IDbContext context, TModel item);
		public abstract void RemoveCacheItem(IDbContext context, TModel item);
	}
}