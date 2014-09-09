#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Transactions;
using FlitBit.Core;
using FlitBit.Data.Cluster;
using FlitBit.Data.CodeContracts;

namespace FlitBit.Data
{
    /// <summary>
    ///     Quasi unit-of-work and scoping mechanism for database operations.
    /// </summary>
    [ContractClass(typeof(ContractsForIDbContext))]
    public interface IDbContext : IInterrogateDisposable
    {
        /// <summary>
        ///     Gets the context behaviors established when the context began.
        /// </summary>
        DbContextBehaviors Behaviors { get; }

        /// <summary>
        ///     Indicates the number of cache attempts during the context.
        /// </summary>
        int CacheAttempts { get; }

        /// <summary>
        ///     Indicates the number of cache hits during the context.
        /// </summary>
        int CacheHits { get; }

        /// <summary>
        ///     Indicates the number of cache puts during the context.
        /// </summary>
        int CachePuts { get; }

        /// <summary>
        ///     Indicates the number of cache removals during the context.
        /// </summary>
        int CacheRemoves { get; }

        /// <summary>
        ///     Indicates the number of queries observed by the context.
        /// </summary>
        int QueryCount { get; }

        /// <summary>
        ///     Indicates the number of objects known to be affected by the context.
        /// </summary>
        int ObjectsAffected { get; }

        /// <summary>
        ///     Indicates the number of objects fetched by the context.
        /// </summary>
        int ObjectsFetched { get; }

        /// <summary>
        ///     Gets the transaction associated with the db context.
        /// </summary>
        Transaction Transaction { get; }

        /// <summary>
        ///     Adds a disposable item to the context for cleanup when the context is disposed.
        /// </summary>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Add<T>(T item)
            where T : IDisposable;

        /// <summary>
        ///     Tries to get an item from the context cache.
        /// </summary>
        /// <typeparam name="T">the item's type</typeparam>
        /// <param name="sharedCache">an ambient shared cache</param>
        /// <param name="key">the item's key</param>
        /// <param name="item">a reference that will contain the ached item upon success.</param>
        /// <returns><em>true</em> if the item was retrieved from the cache; otherwise <em>false</em>.</returns>
        bool TryGet<T>(IClusteredMemory sharedCache, string key, out T item);

        /// <summary>
        ///     Puts an item in the context cache.
        /// </summary>
        /// <typeparam name="T">the item's type</typeparam>
        /// <param name="key">the item's key</param>
        /// <param name="item">the item</param>
        /// <param name="created">indicates whether the item was create within the context or whether it was pre-existing</param>
        /// <param name="promotion">a cache promotion handler invoked upon context completion</param>
        void Put<T>(string key, T item, bool created, ICachePromotionHandler promotion);

        /// <summary>
        ///     Puts an item in the context cache.
        /// </summary>
        /// <typeparam name="T">the item's type</typeparam>
        /// <param name="key">the item's key</param>
        /// <param name="item">the item</param>
        /// <param name="created">indicates whether the item was create within the context or whether it was pre-existing</param>
        /// <param name="ttl">the item's time-to-live</param>
        /// <param name="promotion">a cache promotion handler invoked upon context completion</param>
        void PutWithExpiration<T>(string key, T item, bool created, TimeSpan ttl, ICachePromotionHandler promotion);

        /// <summary>
        ///     Deletes an item from the context cache.
        /// </summary>
        /// <param name="key">the item's key</param>
        /// <param name="promotion">a cache promotion handler invoked upon context completion</param>
        void Delete(string key, ICachePromotionHandler promotion);

        /// <summary>
        ///     Gets the DbProviderHelper for the specified connection.
        /// </summary>
        /// <param name="cn">the connection</param>
        /// <returns></returns>
        DbProviderHelper HelperForConnection(DbConnection cn);

        /// <summary>
        ///     Increments the context's query count.
        /// </summary>
        /// <returns></returns>
        int IncrementQueryCounter();

        /// <summary>
        ///     Increments the context's query count by the specified number.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        int IncrementQueryCounter(int count);

        /// <summary>
        ///     Increments the context's objects affected count by the supecified number.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        int IncrementObjectsAffected(int count);

        /// <summary>
        ///     Increments the context's objects fetched count.
        /// </summary>
        /// <returns></returns>
        int IncrementObjectsFetched();

        /// <summary>
        ///     Increments the context's objects fetched count by the supecified number.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        int IncrementObjectsFetched(int count);

        /// <summary>
        ///     Gets a new connection from ambient connection providers and
        ///     associates it with the context.
        /// </summary>
        /// <param name="connectionName">the connection's name</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     thrown if the connection providers cannot create a connection for the
        ///     specified name.
        /// </exception>
        DbConnection NewConnection(string connectionName);

        /// <summary>
        ///     Gets a strongly typed connection from ambient connection providers
        ///     and associates it with the context.
        /// </summary>
        /// <param name="connectionName">the connection's name</param>
        /// <typeparam name="TDbConnection">the connection's type</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     thrown if the connection providers cannot create a connection for the
        ///     specified name.
        /// </exception>
        /// <exception cref="InvalidCastException">thrown if the connection's type is not a TDbConnection.</exception>
        TDbConnection NewConnection<TDbConnection>(string connectionName)
            where TDbConnection : DbConnection;

        /// <summary>
        ///     Gets or creates a shared connection from the context.
        /// </summary>
        /// <param name="connectionName">the connection's name</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     thrown if the connection providers cannot create a connection for the
        ///     specified name.
        /// </exception>
        DbConnection SharedOrNewConnection(string connectionName);

        /// <summary>
        ///     Gets or creates a strongly typed shared connection from the context.
        /// </summary>
        /// <param name="connectionName">the connection's name</param>
        /// <typeparam name="TDbConnection">the connection's type</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     thrown if the connection providers cannot create a connection for the
        ///     specified name.
        /// </exception>
        /// <exception cref="InvalidCastException">thrown if the connection's type is not a TDbConnection.</exception>
        TDbConnection SharedOrNewConnection<TDbConnection>(string connectionName)
            where TDbConnection : DbConnection;

        /// <summary>
        ///     Prepares the context to be shared across threads. Each shared context must be disposed!
        /// </summary>
        /// <returns></returns>
        IDbContext ShareContext();

        /// <summary>
        ///     Indicates whether the context or transaction completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        ///     Event fired when the db context completes or when it's transaction is completed; whichever occurs later.
        /// </summary>
        event EventHandler<DbContextOrTransactionCompletedEventArgs> OnContextOrTransactionCompleted;
    }

    namespace CodeContracts
    {
        /// <summary>
        ///     CodeContracts Class for IDbContext
        /// </summary>
        [ContractClassFor(typeof(IDbContext))]
        internal abstract class ContractsForIDbContext : IDbContext
        {
            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose() { throw new NotImplementedException(); }

            /// <summary>
            ///     Indicates whether the disposable has been disposed.
            /// </summary>
            public bool IsDisposed { get; private set; }

            /// <summary>
            ///     Gets the context behaviors established when the context began.
            /// </summary>
            public DbContextBehaviors Behaviors { get; private set; }

            /// <summary>
            ///     Indicates the number of cache attempts during the context.
            /// </summary>
            public int CacheAttempts { get; private set; }

            /// <summary>
            ///     Indicates the number of cache hits during the context.
            /// </summary>
            public int CacheHits { get; private set; }

            /// <summary>
            ///     Indicates the number of cache puts during the context.
            /// </summary>
            public int CachePuts { get; private set; }

            /// <summary>
            ///     Indicates the number of cache removals during the context.
            /// </summary>
            public int CacheRemoves { get; private set; }

            /// <summary>
            ///     Indicates the number of queries observed by the context.
            /// </summary>
            public int QueryCount { get; private set; }

            /// <summary>
            ///     Indicates the number of objects known to be affected by the context.
            /// </summary>
            public int ObjectsAffected { get; private set; }

            /// <summary>
            ///     Indicates the number of objects fetched by the context.
            /// </summary>
            public int ObjectsFetched { get; private set; }

            /// <summary>
            ///     Gets the transaction associated with the db context.
            /// </summary>
            public Transaction Transaction { get; private set; }

            /// <summary>
            ///     Adds a disposable item to the context for cleanup when the context is disposed.
            /// </summary>
            /// <param name="item"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public T Add<T>(T item) where T : IDisposable { throw new NotImplementedException(); }

            /// <summary>
            ///     Tries to get an item from the context cache.
            /// </summary>
            /// <typeparam name="T">the item's type</typeparam>
            /// <param name="sharedCache">an ambient shared cache</param>
            /// <param name="key">the item's key</param>
            /// <param name="item">a reference that will contain the ached item upon success.</param>
            /// <returns><em>true</em> if the item was retrieved from the cache; otherwise <em>false</em>.</returns>
            public bool TryGet<T>(IClusteredMemory sharedCache, string key, out T item)
            {
                Contract.Requires<ArgumentNullException>(key != null);
                Contract.Requires<ArgumentException>(key.Length > 0, "Key cannot be empty.");

                throw new NotImplementedException();
            }

            /// <summary>
            ///     Puts an item in the context cache.
            /// </summary>
            /// <typeparam name="T">the item's type</typeparam>
            /// <param name="key">the item's key</param>
            /// <param name="item">the item</param>
            /// <param name="created">indicates whether the item was create within the context or whether it was pre-existing</param>
            /// <param name="promotion">a cache promotion handler invoked upon context completion</param>
            public void Put<T>(string key, T item, bool created, ICachePromotionHandler promotion)
            {
                Contract.Requires<ArgumentNullException>(key != null);
                Contract.Requires<ArgumentException>(key.Length > 0, "Key cannot be empty.");

                throw new NotImplementedException();
            }

            /// <summary>
            ///     Puts an item in the context cache.
            /// </summary>
            /// <typeparam name="T">the item's type</typeparam>
            /// <param name="key">the item's key</param>
            /// <param name="item">the item</param>
            /// <param name="created">indicates whether the item was create within the context or whether it was pre-existing</param>
            /// <param name="ttl">the item's time-to-live</param>
            /// <param name="promotion">a cache promotion handler invoked upon context completion</param>
            public void PutWithExpiration<T>(string key, T item, bool created, TimeSpan ttl,
                ICachePromotionHandler promotion)
            {
                Contract.Requires<ArgumentNullException>(key != null);
                Contract.Requires<ArgumentException>(key.Length > 0, "Key cannot be empty.");

                throw new NotImplementedException();
            }

            /// <summary>
            ///     Deletes an item from the context cache.
            /// </summary>
            /// <param name="key">the item's key</param>
            /// <param name="promotion">a cache promotion handler invoked upon context completion</param>
            public void Delete(string key, ICachePromotionHandler promotion)
            {
                Contract.Requires<ArgumentNullException>(key != null);
                Contract.Requires<ArgumentException>(key.Length > 0, "Key cannot be empty.");

                throw new NotImplementedException();
            }

            /// <summary>
            ///     Gets the DbProviderHelper for the specified connection.
            /// </summary>
            /// <param name="cn">the connection</param>
            /// <returns></returns>
            public DbProviderHelper HelperForConnection(DbConnection cn)
            {
                Contract.Requires<ArgumentNullException>(cn != null);

                throw new NotImplementedException();
            }

            /// <summary>
            ///     Increments the context's query count.
            /// </summary>
            /// <returns></returns>
            public int IncrementQueryCounter() { throw new NotImplementedException(); }

            /// <summary>
            ///     Increments the context's query count by the specified number.
            /// </summary>
            /// <param name="count"></param>
            /// <returns></returns>
            public int IncrementQueryCounter(int count)
            {
                Contract.Requires<ArgumentOutOfRangeException>(count > 0);

                throw new NotImplementedException();
            }

            /// <summary>
            ///     Increments the context's objects affected count by the supecified number.
            /// </summary>
            /// <param name="count"></param>
            /// <returns></returns>
            public int IncrementObjectsAffected(int count)
            {
                Contract.Requires<ArgumentOutOfRangeException>(count > 0);

                throw new NotImplementedException();
            }

            /// <summary>
            ///     Increments the context's objects fetched count.
            /// </summary>
            /// <returns></returns>
            public int IncrementObjectsFetched() { throw new NotImplementedException(); }

            /// <summary>
            ///     Increments the context's objects fetched count by the supecified number.
            /// </summary>
            /// <param name="count"></param>
            /// <returns></returns>
            public int IncrementObjectsFetched(int count)
            {
                Contract.Requires<ArgumentOutOfRangeException>(count > 0);

                throw new NotImplementedException();
            }

            /// <summary>
            ///     Gets a new connection from ambient connection providers and
            ///     associates it with the context.
            /// </summary>
            /// <param name="connectionName">the connection's name</param>
            /// <returns></returns>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     thrown if the connection providers cannot create a connection for the
            ///     specified name.
            /// </exception>
            public DbConnection NewConnection(string connectionName)
            {
                Contract.Requires<ArgumentNullException>(connectionName != null);
                Contract.Requires<ArgumentException>(connectionName.Length > 0, "Connection name cannot be empty.");
                Contract.Ensures(Contract.Result<DbConnection>() != null);

                throw new NotImplementedException();
            }

            /// <summary>
            ///     Gets a strongly typed connection from ambient connection providers
            ///     and associates it with the context.
            /// </summary>
            /// <param name="connectionName">the connection's name</param>
            /// <typeparam name="TDbConnection">the connection's type</typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     thrown if the connection providers cannot create a connection for the
            ///     specified name.
            /// </exception>
            /// <exception cref="InvalidCastException">thrown if the connection's type is not a TDbConnection.</exception>
            public TDbConnection NewConnection<TDbConnection>(string connectionName) where TDbConnection : DbConnection
            {
                Contract.Requires<ArgumentNullException>(connectionName != null);
                Contract.Requires<ArgumentException>(connectionName.Length > 0, "Connection name cannot be empty.");
                Contract.Ensures(Contract.Result<TDbConnection>() != null);

                throw new NotImplementedException();
            }

            /// <summary>
            ///     Gets or creates a shared connection from the context.
            /// </summary>
            /// <param name="connectionName">the connection's name</param>
            /// <returns></returns>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     thrown if the connection providers cannot create a connection for the
            ///     specified name.
            /// </exception>
            public DbConnection SharedOrNewConnection(string connectionName)
            {
                Contract.Requires<ArgumentNullException>(connectionName != null);
                Contract.Requires<ArgumentException>(connectionName.Length > 0, "Connection name cannot be empty.");
                Contract.Ensures(Contract.Result<DbConnection>() != null);

                throw new NotImplementedException();
            }

            /// <summary>
            ///     Gets or creates a strongly typed shared connection from the context.
            /// </summary>
            /// <param name="connectionName">the connection's name</param>
            /// <typeparam name="TDbConnection">the connection's type</typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     thrown if the connection providers cannot create a connection for the
            ///     specified name.
            /// </exception>
            /// <exception cref="InvalidCastException">thrown if the connection's type is not a TDbConnection.</exception>
            public TDbConnection SharedOrNewConnection<TDbConnection>(string connectionName)
                where TDbConnection : DbConnection
            {
                Contract.Requires<ArgumentNullException>(connectionName != null);
                Contract.Requires<ArgumentException>(connectionName.Length > 0, "Connection name cannot be empty.");
                Contract.Ensures(Contract.Result<TDbConnection>() != null);

                throw new NotImplementedException();
            }

            /// <summary>
            ///     Prepares the context to be shared across threads. Each shared context must be disposed!
            /// </summary>
            /// <returns></returns>
            public IDbContext ShareContext()
            {
                Contract.Ensures(Contract.Result<IDbContext>() != null);

                throw new NotImplementedException();
            }

            /// <summary>
            ///     Indicates whether the context or transaction completed.
            /// </summary>
            public bool IsCompleted { get { throw new NotImplementedException(); } }

            public event EventHandler<DbContextOrTransactionCompletedEventArgs> OnContextOrTransactionCompleted
            {
                add
                {
                    Contract.Requires<InvalidOperationException>(!IsCompleted);

                    throw new NotImplementedException();
                }
                remove
                {
                    Contract.Requires<InvalidOperationException>(!IsCompleted);

                    throw new NotImplementedException();
                }
            }
        }
    }
}