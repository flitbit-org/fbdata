#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data.Common;
using FlitBit.Core;
using FlitBit.Core.Parallel;

namespace FlitBit.Data
{
	public interface IDbContext : IInterrogateDisposable, IParallelShared
	{
		DbContextBehaviors Behaviors { get; }
		int CacheAttempts { get; }
		int CacheHits { get; }
		int CachePuts { get; }
		int CacheRemoves { get; }
		int QueryCount { get; }

		T Add<T>(T item)
			where T : IDisposable;

		C EnsureCache<K, C>(K key)
			where C : new();

		DbProviderHelper HelperForConnection(DbConnection cn);
		int IncrementQueryCounter();
		int IncrementQueryCounter(int count);
		DbConnection NewConnection(string connection);

		TConnection NewConnection<TConnection>(string connectionName)
			where TConnection : DbConnection, new();

		void PutCacheItem<TCacheKey, TItemKey, TItem>(TCacheKey cacheKey, TItem item, TItemKey key,
			Func<TItemKey, TItem, TItem> updateCachedItem);

		void RemoveCacheItem<TCacheKey, TItemKey, TItem>(TCacheKey cacheKey, TItem item, TItemKey key);
		DbConnection SharedOrNewConnection(string connection);

		TConnection SharedOrNewConnection<TConnection>(string connectionName)
			where TConnection : DbConnection, new();

		bool TryGetCacheItem<TCacheKey, TItemKey, TItem>(TCacheKey cacheKey, TItemKey key, out TItem item);
	}
}