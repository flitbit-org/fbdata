#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;
using FlitBit.Data.Repositories;

namespace FlitBit.Data
{
	public partial class DbContext
	{
		int _cacheAttempts = 0;
		int _cacheHits = 0;
		int _cachePuts = 0;
		int _cacheRemoves = 0;

		Lazy<ConcurrentDictionary<object, ContextCache>> _localCaches =
			new Lazy<ConcurrentDictionary<object, ContextCache>>(LazyThreadSafetyMode.PublicationOnly);

		#region IDbContext Members

		public int CachePuts { get { return Thread.VolatileRead(ref _cachePuts); } }

		public int CacheAttempts { get { return Thread.VolatileRead(ref _cacheAttempts); } }

		public int CacheHits { get { return Thread.VolatileRead(ref _cacheHits); } }

		public int CacheRemoves { get { return Thread.VolatileRead(ref _cacheRemoves); } }

		public int IncrementCacheAttempts()
		{
			return Interlocked.Increment(ref _cacheAttempts);
		}

		public C EnsureCache<K, C>(K key, Func<IDbContext, K, C> factory)
			where C : ContextCache
		{
			Contract.Requires<InvalidOperationException>(!Behaviors.HasFlag(DbContextBehaviors.DisableCaching));

			if (_parent != null && _behaviors.HasFlag(DbContextBehaviors.DisableInheritedCache))
			{
				return _parent.EnsureCache(key, factory);
			}
			var caches = _localCaches.Value;
			C cache = default(C);
			while (true)
			{
				ContextCache res;
				if (caches.TryGetValue(key, out res))
				{
					return (C) res;
				}
				if (cache == null)
				{
					cache = factory(this, key);
				}
				if (caches.TryAdd(key, cache))
				{
					return cache;
				}
			}
		}

		public int IncrementCacheHits()
		{
			return Interlocked.Increment(ref _cacheHits);
		}
		public int IncrementCachePuts()
		{
			return Interlocked.Increment(ref _cachePuts);
		}
		public int IncrementCacheRemoves()
		{
			return Interlocked.Increment(ref _cacheRemoves);
		}
		
		#endregion
	}
}