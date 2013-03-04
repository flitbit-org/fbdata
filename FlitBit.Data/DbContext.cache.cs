#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;

namespace FlitBit.Data
{
	public partial class DbContext
	{
		Lazy<ConcurrentDictionary<object, object>> _localCaches = new Lazy<ConcurrentDictionary<object, object>>(LazyThreadSafetyMode.PublicationOnly);
		int _cacheAttempts = 0;
		int _cachePuts = 0;
		int _cacheHits = 0;
		int _cacheRemoves = 0;

		public void PutCacheItem<TCacheKey, TItemKey, TItem>(TCacheKey cacheKey, TItem item, TItemKey key, Func<TItemKey, TItem, TItem> updateCachedItem) 
		{
			Contract.Requires<InvalidOperationException>(!Behaviors.HasFlag(DbContextBehaviors.DisableCaching));

			var cache = EnsureCache<TCacheKey, ConcurrentDictionary<TItemKey, TItem>>(cacheKey);
			cache.AddOrUpdate(key, item, updateCachedItem);
			Interlocked.Increment(ref _cachePuts);
		}

		public void RemoveCacheItem<TCacheKey, TItemKey, TItem>(TCacheKey cacheKey, TItem item, TItemKey key)
		{
			Contract.Requires<InvalidOperationException>(!Behaviors.HasFlag(DbContextBehaviors.DisableCaching));

			var cache = EnsureCache<TCacheKey, ConcurrentDictionary<TItemKey, TItem>>(cacheKey);
			TItem unused;
			if (cache.TryRemove(key, out unused))
			{
				Interlocked.Increment(ref _cacheRemoves);
			}
		}

		public bool TryGetCacheItem<TCacheKey, TItemKey, TItem>(TCacheKey cacheKey, TItemKey key, out TItem item)
		{
			Contract.Requires<InvalidOperationException>(!Behaviors.HasFlag(DbContextBehaviors.DisableCaching));

			var cache = EnsureCache<TCacheKey, ConcurrentDictionary<TItemKey, TItem>>(cacheKey);
			Interlocked.Increment(ref _cacheAttempts);
			if (cache.TryGetValue(key, out item))
			{
				Interlocked.Increment(ref _cacheHits);
				return true;
			}
			item = default(TItem);
			return false;
		}	

		public C EnsureCache<K, C>(K key)
			where C : new()
		{
			Contract.Requires<InvalidOperationException>(!Behaviors.HasFlag(DbContextBehaviors.DisableCaching));

			if (_parent != null && _behaviors.HasFlag(DbContextBehaviors.DisableInheritedCache))
			{
				return _parent.EnsureCache<K, C>(key);
			}
			else
			{
				var caches = _localCaches.Value;
				return (C)caches.GetOrAdd(key, ignored => new C());
			}
		}

		public int CachePuts
		{
			get { return Thread.VolatileRead(ref _cachePuts); }
		}

		public int CacheAttempts
		{
			get { return Thread.VolatileRead(ref _cacheAttempts); }
		}

		public int CacheHits
		{
			get { return Thread.VolatileRead(ref _cacheHits); }
		}

		public int CacheRemoves
		{
			get { return Thread.VolatileRead(ref _cacheRemoves); }
		}
	}
}
