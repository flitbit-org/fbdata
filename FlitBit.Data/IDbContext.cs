﻿#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data.Common;
using FlitBit.Core;
using FlitBit.Core.Parallel;
using FlitBit.Data.Repositories;

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
		int ObjectsAffected { get; }
		int ObjectsFetched { get; }

		T Add<T>(T item)
			where T : IDisposable;

		C EnsureCache<K, C>(K key, Func<IDbContext, K, C> factory)
			where C : ContextCache;

		DbProviderHelper HelperForConnection(DbConnection cn);
		int IncrementQueryCounter();
		int IncrementQueryCounter(int count);
		int IncrementObjectsAffected(int count);
		int IncrementObjectsFetched(int count);
		int IncrementObjectsFetched();

		int IncrementCacheAttempts();

		int IncrementCacheHits();

		int IncrementCachePuts();
		int IncrementCacheRemoves();

		DbConnection NewConnection(string connection);

		TConnection NewConnection<TConnection>(string connectionName)
			where TConnection : DbConnection, new();

		DbConnection SharedOrNewConnection(string connectionName);

		TConnection SharedOrNewConnection<TConnection>(string connectionName)
			where TConnection : DbConnection, new();
	}
}