using System;

namespace FlitBit.Data.Repositories
{
	public class ContextCacheByIdentityKeyCacheHandler<TModel, TCacheKey, TItemKey> : AbstractCacheHandler<TModel>
	{
		private readonly TCacheKey _cacheKey;
		private readonly Func<TModel, TItemKey> _calculateKey;

		public ContextCacheByIdentityKeyCacheHandler(TCacheKey cacheKey, Func<TModel, TItemKey> calculateKey)
		{
			_cacheKey = cacheKey;
			_calculateKey = calculateKey;
		}

		public override void PerformUpdateCacheItem(IDbContext context, TModel item)
		{
			context.PutCacheItem(_cacheKey, item, _calculateKey(item), (k, v) => item);
		}

		public override void RemoveCacheItem(IDbContext context, TModel item)
		{
			context.RemoveCacheItem(_cacheKey, item, _calculateKey(item));
		}
	}
}