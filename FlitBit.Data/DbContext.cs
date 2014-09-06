#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Transactions;
using FlitBit.Core;
using FlitBit.Core.Log;
using FlitBit.Data.Cluster;
using FlitBit.Data.Properties;

namespace FlitBit.Data
{
    /// <summary>
    /// Default implementation of IDbContext.
    /// </summary>
    public partial class DbContext : Disposable, IDbContext
    {
        readonly DbContextBehaviors _behaviors;

        readonly ConcurrentDictionary<string, IConnection> _connections =
            new ConcurrentDictionary<string, IConnection>();

        int _disposers = 1;

        readonly ConcurrentDictionary<DbConnection, DbProviderHelper> _helpers =
            new ConcurrentDictionary<DbConnection, DbProviderHelper>();

        IDbContext _parent;
        int _queryCount;
        int _objectsAffected;
        int _objectsFetched;
        ICleanupScope _scope;
        readonly TransactionScope _txScope;
        int _txCompleted;

        /// <summary>
        /// Creaets a new instance.
        /// </summary>
        public DbContext()
            : this(Current, Transaction.Current, DbContextBehaviors.Default)
        {}

        DbContext(IDbContext parent, Transaction transaction, TransactionScope scope, DbContextBehaviors behaviors)
            : this(parent, transaction, behaviors)
        {
            _txScope = scope;
        }

        /// <summary>
        /// Creates a new instance with the specified parent, transaction and behaviors.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="transaction"></param>
        /// <param name="behaviors"></param>
        public DbContext(IDbContext parent, Transaction transaction, DbContextBehaviors behaviors)
        {
            _behaviors = behaviors;
            this.Transaction = transaction;
            if (parent != null)
            {
                _parent = parent.ShareContext();
            }
            _scope = new CleanupScope(this, true);
            if (!behaviors.HasFlag(DbContextBehaviors.NoContextFlow))
            {
                DbContextFlowProvider.Push(this);
            }
            if (Transaction == null)
            {
                _scope.AddAction(() => ContextOrTransactionCompleted(this, null));
            }
            else
            {
                Transaction.TransactionCompleted += ContextOrTransactionCompleted;
            }
        }

        internal void Complete()
        {
            if (_txScope != null)
            {
                _txScope.Complete();
            }
        }

        /// <summary>
        /// Indicates whether the context or transaction completed.
        /// </summary>
        public bool IsCompleted { get { return Thread.VolatileRead(ref _txCompleted) > 0; } }

        public Transaction Transaction { get; private set; }

        /// <summary>
        /// Event fired when the db context completes or when it's transaction is completed; whichever occurs later.
        /// </summary>
        public event EventHandler<DbContextOrTransactionCompletedEventArgs> OnContextOrTransactionCompleted
        {
            add
            {
                _onContextOrTransactionCompleted += value;
            }
            remove
            {
                _onContextOrTransactionCompleted -= value;
            }
        }

        private event EventHandler<DbContextOrTransactionCompletedEventArgs> _onContextOrTransactionCompleted;
        
        void ContextOrTransactionCompleted(object sender, TransactionEventArgs ev)
        {
            if (!IsCompleted)
            {
                try
                {
                    var hasTrans = (ev != null && ev.Transaction != null);
                    var status = hasTrans
                                     ? ev.Transaction.TransactionInformation.Status
                                     : TransactionStatus.Committed;
                    if (_onContextOrTransactionCompleted != null)
                    {
                        var args = new DbContextOrTransactionCompletedEventArgs(hasTrans, status);
                        _onContextOrTransactionCompleted(this, args);
                    }
                    PerformCachePromotion(hasTrans, status);
                }
                finally
                {
                    Thread.VolatileWrite(ref _txCompleted, 1);
                }
            }
        }

        protected override bool PerformDispose(bool disposing)
        {
            if (disposing && Interlocked.Decrement(ref _disposers) > 0)
            {
                return false;
            }

            if (!DbContextFlowProvider.TryPop(this)
                && DbTraceEvents.ShouldTrace(TraceEventType.Warning))
            {
                try
                {
                    DbTraceEvents.OnTraceEvent(this, TraceEventType.Warning,
                        Resources.Err_DbContextStackDisposedOutOfOrder);
                }
// ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                    /* possibly in GC thread */
                }
            }
            if (disposing)
            {
                Util.Dispose(ref _scope);
                Util.Dispose(ref _parent);
            }
            return true;
        }

        #region IDbContext Members

        public DbContextBehaviors Behaviors { get { return _behaviors; } }

        public DbConnection SharedOrNewConnection(string connectionName)
        {
            return InnerSharedOrNewConnection<DbConnection>(connectionName);
        }

        TDbConnection InnerSharedOrNewConnection<TDbConnection>(string name)
            where TDbConnection: DbConnection
        {
            while (true)
            {
                IConnection cn;
                if (_connections.TryGetValue(name, out cn))
                {
                    if (cn.CanShareConnection)
                    {
                        return (TDbConnection)cn.UntypedDbConnection;
                    }
                    return InnerNewConnection<TDbConnection>(name).DbConnection;
                }

                var ours = ConnectionProviders.Instance.GetConnection<TDbConnection>(name);
                var cnObj = ours.DbConnection;
                var helper = HelperForConnection(cnObj);
                if (helper != null
                    && helper.SupportsMultipleActiveResultSets(cnObj))
                {
                    ours = new DefaultConnection<TDbConnection>(name, ours.DbConnection, true);
                }
                if (_connections.TryAdd(name, ours))
                {
                    var disposals = 0;
                    cnObj.Disposed += (sender, e) => Interlocked.Increment(ref disposals);
                    _scope.AddAction(
                        () =>
                        {
                            if (Thread.VolatileRead(ref disposals) == 0
                                && cnObj.State != ConnectionState.Closed)
                            {
                                cnObj.Close();
                            }
                        });

                    return cnObj;
                }
                cnObj.Dispose();
            }
        }

        IConnection<TDbConnection> InnerNewConnection<TDbConnection>(string name)
            where TDbConnection : DbConnection
        {
            var cn = ConnectionProviders.Instance.GetConnection<TDbConnection>(name);
            
            // Ensure the connection will close when the dbcontext's scope closes.
            var cnObj = cn.UntypedDbConnection;
            var disposals = 0;
            cnObj.Disposed += (sender, e) => Interlocked.Increment(ref disposals);
            _scope.AddAction(
                () =>
                {
                    if (Thread.VolatileRead(ref disposals) == 0
                        && cnObj.State != ConnectionState.Closed)
                    {
                        cnObj.Close();
                    }
                });
            return cn;
        }

        void TryRemoveSharedConnection(string name, DbConnection cn)
        {
            throw new NotImplementedException();
        }

        public DbConnection NewConnection(string connectionName)
        {
            return InnerNewConnection<DbConnection>(connectionName).UntypedDbConnection;
        }

        public T Add<T>(T item) where T : IDisposable { return (T)_scope.Add(((IDisposable)item)); }

        public TDbConnection SharedOrNewConnection<TDbConnection>(string connectionName)
            where TDbConnection : DbConnection
        {
            return InnerSharedOrNewConnection<TDbConnection>(connectionName);
        }

        public TDbConnection NewConnection<TDbConnection>(string connectionName)
            where TDbConnection : DbConnection
        {
            return InnerNewConnection<TDbConnection>(connectionName).DbConnection;
        }


        public IDbContext ShareContext()
        {
            Interlocked.Increment(ref _disposers);
            return this;
        }

        public int QueryCount { get { return Thread.VolatileRead(ref _queryCount); } }

        public int ObjectsAffected { get { return Thread.VolatileRead(ref _objectsAffected); } }

        public int ObjectsFetched { get { return Thread.VolatileRead(ref _objectsFetched); } }

        public int IncrementQueryCounter() { return Interlocked.Increment(ref _queryCount); }

        public int IncrementQueryCounter(int count) { return Interlocked.Add(ref _queryCount, count); }

        public int IncrementObjectsAffected(int count) { return Interlocked.Add(ref _objectsAffected, count); }
        public int IncrementObjectsFetched(int count) { return Interlocked.Add(ref _objectsFetched, count); }
        public int IncrementObjectsFetched() { return Interlocked.Increment(ref _objectsFetched); }

        public DbProviderHelper HelperForConnection(DbConnection cn)
        {
            DbProviderHelper res;
            if (!_helpers.TryGetValue(cn, out res))
            {
                res = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
                _helpers.TryAdd(cn, res);
            }
            return res;
        }

        #endregion
    }
}