#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data.Common;
using System.Transactions;
using FlitBit.Core;
using FlitBit.Data.Cluster;

namespace FlitBit.Data
{
  public interface IDbContext : IInterrogateDisposable
  {
    DbContextBehaviors Behaviors { get; }
    int CacheAttempts { get; }
    int CacheHits { get; }
    int CachePuts { get; }
    int CacheRemoves { get; }
    int QueryCount { get; }
    int ObjectsAffected { get; }
    int ObjectsFetched { get; }

    /// <summary>
    /// Gets the transaction associated with the db context.
    /// </summary>
    Transaction Transaction { get; }

    /// <summary>
    /// Adds a disposable item to the context for cleanup when the context is disposed.
    /// </summary>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Add<T>(T item)
      where T : IDisposable;

    /// <summary>
    /// Tries to get an item from the context cache.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    bool TryGet<T>(IClusteredMemory mem, string key, out T item);

    /// <summary>
    /// Puts an item in the context cache.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="item"></param>
    /// <param name="created"></param>
    /// <param name="promotion"></param>
    void Put<T>(string key, T item, bool created, ICachePromotionHandler promotion);

    /// <summary>
    /// Puts an item in the context cache.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="item"></param>
    /// <param name="created"></param>
    /// <param name="ttl"></param>
    /// <param name="promotion"></param>
    void PutWithExpiration<T>(string key, T item, bool created, TimeSpan ttl, ICachePromotionHandler promotion);

    /// <summary>
    /// Deletes an item from the context cache.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="promotion"></param>
    void Delete(string key, ICachePromotionHandler promotion);

    DbProviderHelper HelperForConnection(DbConnection cn);
    int IncrementQueryCounter();
    int IncrementQueryCounter(int count);
    int IncrementObjectsAffected(int count);
    int IncrementObjectsFetched(int count);
    int IncrementObjectsFetched();
    
    DbConnection NewConnection(string connection);

    TConnection NewConnection<TConnection>(string connectionName)
      where TConnection : DbConnection;

    DbConnection SharedOrNewConnection(string connectionName);

    TConnection SharedOrNewConnection<TConnection>(string connectionName)
      where TConnection : DbConnection;

    /// <summary>
    ///   Prepares the context to be shared across threads. Each result must be disposed.
    /// </summary>
    /// <returns></returns>
    IDbContext ShareContext();

    /// <summary>
    /// Event fired when the db context completes or when it's transaction is completed; whichever occurs later.
    /// </summary>
    event EventHandler<DbContextOrTransactionCompletedEventArgs> OnContextOrTransactionCompleted;
  }
}