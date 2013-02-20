using System;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics.Contracts;
using System.Threading;
using FlitBit.Core;
using FlitBit.Core.Parallel;

namespace FlitBit.Data
{
	public class DbContext: Disposable, IDbContext
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

		ConcurrentDictionary<string, IDbConnection> _connections = new ConcurrentDictionary<string, IDbConnection>();
		int _disposers = 1;

		public DbContext()
		{
			ContextFlow.Push<DbContext>(this);
		}

		public IDbConnection NewOrSharedConnection(string connection)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentException>(connection.Length > 0);

			IDbConnection cn, capture = null;
			cn = _connections.GetOrAdd(connection, cs => {
				capture = DbExtensions.CreateAndOpenConnection(cs);
				return capture;
			});
			if (capture != null && !Object.ReferenceEquals(cn, capture))
			{
				capture.Dispose();
			}
			return cn;
		}

		protected override bool PerformDispose(bool disposing)
		{
			if (disposing && Interlocked.Decrement(ref _disposers) > 0)
			{
				return false;
			}
			ContextFlow.TryPop<DbContext>(this);
			return true;
		}

		public object ParallelShare()
		{
			Interlocked.Increment(ref _disposers);
			return this;
		}

		public IDbConnection NewConnection(string connection)
		{
			throw new NotImplementedException();
		}

		public IDbConnection Add<T>(IDbConnection cn)
		{
			throw new NotImplementedException();
		}

		public IDbCommand Add<T>(IDbCommand cm)
		{
			throw new NotImplementedException();
		}	 

		public T Add<T>(T contextual) where T : IDbContextual
		{
			throw new NotImplementedException();
		}
	}
}
