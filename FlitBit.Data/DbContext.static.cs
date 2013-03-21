#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using FlitBit.Core;
using FlitBit.Core.Parallel;

namespace FlitBit.Data
{
	public partial class DbContext : Disposable, IDbContext
	{
		/// <summary>
		///   Gets the current "ambient" db context.
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
		///   Creates a new context.
		/// </summary>
		/// <param name="behaviors">indicates the context's behaviors</param>
		/// <returns>a db context</returns>
		public static IDbContext NewContext(DbContextBehaviors behaviors)
		{
			if (behaviors.HasFlag(DbContextBehaviors.NoContextFlow))
			{
				return new DbContext(null, behaviors);
			}
			else
			{
				return new DbContext(Current, behaviors);
			}
		}

		/// <summary>
		///   Creates a new context.
		/// </summary>
		/// <returns>a db context</returns>
		public static IDbContext NewContext()
		{
			return new DbContext(Current, DbContextBehaviors.Default);
		}

		/// <summary>
		///   Shares the ambient context if it exists; otherwise, creates a new context.
		/// </summary>
		/// <returns>a db context</returns>
		public static IDbContext SharedOrNewContext()
		{
			IDbContext ambient;
			return (ContextFlow.TryPeek<IDbContext>(out ambient))
				? (IDbContext) ambient.ParallelShare()
				: new DbContext(null, DbContextBehaviors.Default);
		}
	}
}