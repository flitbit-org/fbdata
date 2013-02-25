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
	public class DbContext : Disposable, IDbContext
	{
		/// <summary>
		/// Gets the current "ambient" db context.
		/// </summary>
		public static IDbContext Current
		{
			get
			{
				IDbContext ambient;
				return (ContextFlow.TryPeek<IDbContext>(out ambient)) ? ambient : default(IDbContext);
			}
		}

		/// <summary>
		/// Shares the ambient context if it exists; otherwise, creates a new context.
		/// </summary>
		/// <returns>a db context</returns>
		public static IDbContext SharedOrNewContext()
		{
			IDbContext ambient;
			return (ContextFlow.TryPeek<IDbContext>(out ambient))
				? (IDbContext)ambient.ParallelShare()
				: new DbContext();
		}

		/// <summary>
		/// Creates a new context.
		/// </summary>
		/// <returns>a db context</returns>
		public static IDbContext NewContext()
		{
			return new DbContext();
		}

		ConcurrentDictionary<string, DbConnection> _connections = new ConcurrentDictionary<string, DbConnection>();
		ICleanupScope _scope;
		int _disposers = 1;

		public DbContext()
		{
			_scope = new CleanupScope(this, true);
			ContextFlow.Push<IDbContext>(this);
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
				if (!Object.ReferenceEquals(cn, capture))
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
				catch { /* no errors surfaced in GC thread */ }
			}
			Util.Dispose(ref _scope);
			return true;
		}

		public object ParallelShare()
		{
			Interlocked.Increment(ref _disposers);
			return this;
		}

		public T Add<T>(T item) where T : IDisposable
		{
			return _scope.Add(item);
		}

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
				if (!Object.ReferenceEquals(cn, capture))
				{						
					capture.Dispose();
				}
			}
			return (TConnection)cn;
		}

		public TConnection NewConnection<TConnection>(string connectionName)
			where TConnection : DbConnection, new()
		{
			var cn = DbExtensions.CreateConnection<TConnection>(connectionName);
			var disposals = 0;
			cn.Disposed += (sender, e) =>	Interlocked.Increment(ref disposals);
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
	}
}
