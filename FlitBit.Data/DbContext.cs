#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using FlitBit.Core;
using FlitBit.Core.Parallel;
using FlitBit.Data.Properties;

namespace FlitBit.Data
{
	public partial class DbContext : Disposable, IDbContext
	{
		DbContextBehaviors _behaviors;
		ConcurrentDictionary<string, DbConnection> _connections = new ConcurrentDictionary<string, DbConnection>();
		int _disposers = 1;

		ConcurrentDictionary<DbConnection, DbProviderHelper> _helpers =
			new ConcurrentDictionary<DbConnection, DbProviderHelper>();

		IDbContext _parent;
		int _queryCount = 0;
		ICleanupScope _scope;

		public DbContext()
			: this(Current, DbContextBehaviors.Default) { }

		public DbContext(IDbContext parent, DbContextBehaviors behaviors)
		{
			_behaviors = behaviors;
			if (parent != null)
			{
				_parent = (IDbContext) parent.ParallelShare();
			}
			_scope = new CleanupScope(this, true);
			if (!behaviors.HasFlag(DbContextBehaviors.NoContextFlow))
			{
				ContextFlow.Push<IDbContext>(this);
			}
		}

		public DbContextBehaviors Behaviors
		{
			get { return _behaviors; }
		}

		public DbConnection SharedOrNewConnection(string connectionName)
		{
			Contract.Requires<ArgumentNullException>(connectionName != null);
			Contract.Requires<ArgumentException>(connectionName.Length > 0);

			DbConnection cn, capture = null;
			cn = _connections.GetOrAdd(connectionName,
																cs =>
																	{
																		capture = NewConnection(connectionName);
																		return capture;
																	});
			if (capture != null)
			{
				if (!ReferenceEquals(cn, capture))
				{
					capture.Dispose();
				}
			}
			return cn;
		}

		public DbConnection NewConnection(string connectionName)
		{
			var cn = DbExtensions.CreateConnection(connectionName);
			var disposals = 0;
			cn.Disposed += (sender, e) => Interlocked.Increment(ref disposals);
			_scope.AddAction(
											 () =>
												 {
													if (Thread.VolatileRead(ref disposals) == 0)
													{
														cn.Close();
													}
												 });
			return cn;
		}

		public T Add<T>(T item) where T : IDisposable { return _scope.Add(item); }

		public TConnection SharedOrNewConnection<TConnection>(string connectionName)
			where TConnection : DbConnection, new()
		{
			DbConnection cn, capture = null;
			cn = _connections.GetOrAdd(connectionName,
																cs =>
																	{
																		capture = NewConnection<TConnection>(connectionName);
																		return capture;
																	});
			if (capture != null)
			{
				if (!ReferenceEquals(cn, capture))
				{
					capture.Dispose();
				}
			}
			return (TConnection) cn;
		}

		public TConnection NewConnection<TConnection>(string connectionName)
			where TConnection : DbConnection, new()
		{
			var cn = DbExtensions.CreateConnection<TConnection>(connectionName);
			var disposals = 0;
			cn.Disposed += (sender, e) => Interlocked.Increment(ref disposals);
			_scope.AddAction(
											 () =>
												 {
													if (Thread.VolatileRead(ref disposals) == 0)
													{
														cn.Close();
													}
												 });
			return cn;
		}

		public object ParallelShare()
		{
			Interlocked.Increment(ref _disposers);
			return this;
		}

		public int QueryCount
		{
			get { return Thread.VolatileRead(ref _queryCount); }
		}

		public int IncrementQueryCounter() { return Interlocked.Increment(ref _queryCount); }

		public int IncrementQueryCounter(int count) { return Interlocked.Add(ref _queryCount, count); }

		public DbProviderHelper HelperForConnection(DbConnection cn)
		{
			var res = default(DbProviderHelper);
			if (!_helpers.TryGetValue(cn, out res))
			{
				res = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				_helpers.TryAdd(cn, res);
			}
			return res;
		}

		protected override bool PerformDispose(bool disposing)
		{
			if (disposing && Interlocked.Decrement(ref _disposers) > 0)
			{
				return false;
			}
			if (!ContextFlow.TryPop<IDbContext>(this) && DbTraceEvents.ShouldTrace(TraceEventType.Warning))
			{
				try
				{
					DbTraceEvents.OnTraceEvent(this, TraceEventType.Warning, Resources.Err_DbContextStackDisposedOutOfOrder);
				}
				catch
				{
					/* no errors surfaced in GC thread */
				}
			}
			if (disposing)
			{
				Util.Dispose(ref _scope);
				Util.Dispose(ref _parent);
			}
			return true;
		}
	}
}