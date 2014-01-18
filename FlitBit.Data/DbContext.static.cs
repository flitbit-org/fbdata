#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using FlitBit.Core;
using FlitBit.Core.Parallel;

namespace FlitBit.Data
{
	public partial class DbContext : Disposable, IDbContext
	{
    internal class DbContextFlowProvider : IContextFlowProvider
    {
      static readonly Lazy<DbContextFlowProvider> Provider =
        new Lazy<DbContextFlowProvider>(CreateAndRegisterContextFlowProvider, LazyThreadSafetyMode.ExecutionAndPublication);

      static DbContextFlowProvider CreateAndRegisterContextFlowProvider()
      {
        var res = new DbContextFlowProvider();
        ContextFlow.RegisterProvider(res);
        return res;
      }

      [ThreadStatic]
      static Stack<IDbContext> __scopes;

      public DbContextFlowProvider()
      {
        this.ContextKey = Guid.NewGuid();
      }

      public Guid ContextKey
      {
        get;
        private set;
      }

      public object Capture()
      {
        var top = Peek();
        if (top != null)
        {
          return top.ShareContext();
        }
        return null;
      }

      public void Attach(ContextFlow context, object captureKey)
      {
        var scope = (captureKey as IDbContext);
        if (scope != null)
        {
          if (__scopes == null)
          {
            __scopes = new Stack<IDbContext>();
          }
          if (__scopes.Count > 0)
          {
            ReportAndClearOrphanedScopes(__scopes);
          }
          __scopes.Push(scope);
        }
      }

      private void ReportAndClearOrphanedScopes(Stack<IDbContext> scopes)
      {
        scopes.Clear();
      }

      public void Detach(ContextFlow context, object captureKey)
      {
        var scope = (captureKey as IDbContext);
        if (scope != null)
        {
          scope.Dispose();
        }
      }

      internal static void Push(IDbContext scope)
      {
        var dummy = Provider.Value;
        if (__scopes == null)
        {
          __scopes = new Stack<IDbContext>();
        }
        __scopes.Push(scope);
      }

      internal static bool TryPop(IDbContext scope)
      {
        if (__scopes != null && __scopes.Count > 0)
        {
          if (ReferenceEquals(__scopes.Peek(), scope))
          {
            __scopes.Pop();
            return true;
          }
        }
        return false;
      }

      internal static IDbContext Pop()
      {
        if (__scopes != null && __scopes.Count > 0)
        {
          return __scopes.Pop();
        }
        return default(DbContext);
      }


      internal static IDbContext Peek()
      {
        if (__scopes != null && __scopes.Count > 0)
        {
          return __scopes.Peek();
        }
        return default(DbContext);
      }
    }


		/// <summary>
		///   Gets the current "ambient" db context.
		/// </summary>
		public static IDbContext Current
		{
			get
			{
			  return DbContextFlowProvider.Peek();
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
		  IDbContext ambient = DbContextFlowProvider.Peek();
			return (ambient != null)
				? ambient.ShareContext()
				: new DbContext(null, DbContextBehaviors.Default);
		}
	}
}