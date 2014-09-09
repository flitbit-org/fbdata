#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Transactions;
using FlitBit.Core.Parallel;

namespace FlitBit.Data
{
    public partial class DbContext
    {
        internal class DbContextFlowProvider : IContextFlowProvider
        {
            static readonly Lazy<DbContextFlowProvider> __provider =
                new Lazy<DbContextFlowProvider>(CreateAndRegisterContextFlowProvider,
                    LazyThreadSafetyMode.ExecutionAndPublication);

            static void EnsureContextFlowProviderExists()
            {
                if (!__provider.IsValueCreated)
                {
                    var provider = __provider.Value;
                    Contract.Assert(provider != null);
                }
            }

            class DbContextCapture
            {
                readonly IDbContext _context;
                readonly Transaction _transaction;

                internal DbContextCapture(IDbContext ctx)
                {
                    _context = ctx;
                    _transaction = ctx.Transaction;
                }

                public DbContext MakeDependent()
                {
                    if (_transaction != null)
                    {
                        var txScope =
                            new TransactionScope(
                                _transaction.DependentClone(DependentCloneOption.BlockCommitUntilComplete));
                        return new DbContext(_context, Transaction.Current, txScope, DbContextBehaviors.Default);
                    }
                    return new DbContext(_context, Transaction.Current, DbContextBehaviors.Default);
                }
            }

            static DbContextFlowProvider CreateAndRegisterContextFlowProvider()
            {
                var res = new DbContextFlowProvider();
                ContextFlow.RegisterProvider(res);
                return res;
            }

            [ThreadStatic]
            static Stack<IDbContext> __scopes;

            public DbContextFlowProvider() { this.ContextKey = Guid.NewGuid(); }

            public Guid ContextKey { get; private set; }

            public object Capture()
            {
                var top = Peek();
                if (top != null)
                {
                    return new DbContextCapture(top);
                }
                return null;
            }

            public object Attach(ContextFlow context, object capture)
            {
                var scope = (capture as DbContextCapture);
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
                    var res = scope.MakeDependent();
                    __scopes.Push(res);
                    return res;
                }
                return null;
            }

            void ReportAndClearOrphanedScopes(Stack<IDbContext> scopes)
            {
                // TODO: report the orphaned scopes.
                scopes.Clear();
            }

            public void Detach(ContextFlow context, object attachement, Exception err)
            {
                var scope = (attachement as DbContext);
                if (scope != null)
                {
                    if (err == null
                        && scope.Transaction != null)
                    {
                        scope.Complete();
                    }
                    scope.Dispose();
                }
            }

            internal static void Push(IDbContext scope)
            {
                EnsureContextFlowProviderExists();
                if (__scopes == null)
                {
                    __scopes = new Stack<IDbContext>();
                }
                __scopes.Push(scope);
            }

            internal static bool TryPop(IDbContext scope)
            {
                if (__scopes != null
                    && __scopes.Count > 0)
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
                if (__scopes != null
                    && __scopes.Count > 0)
                {
                    return __scopes.Pop();
                }
                return default(DbContext);
            }

            internal static IDbContext Peek()
            {
                if (__scopes != null
                    && __scopes.Count > 0)
                {
                    return __scopes.Peek();
                }
                return default(DbContext);
            }
        }

        /// <summary>
        ///     Gets the current "ambient" db context.
        /// </summary>
        public static IDbContext Current { get { return DbContextFlowProvider.Peek(); } }

        /// <summary>
        ///     Creates a new context.
        /// </summary>
        /// <param name="behaviors">indicates the context's behaviors</param>
        /// <returns>a db context</returns>
        public static IDbContext NewContext(DbContextBehaviors behaviors)
        {
            if (behaviors.HasFlag(DbContextBehaviors.NoContextFlow))
            {
                return new DbContext(null, Transaction.Current, behaviors);
            }
            return new DbContext(Current, Transaction.Current, behaviors);
        }

        /// <summary>
        ///     Creates a new context.
        /// </summary>
        /// <returns>a db context</returns>
        public static IDbContext NewContext()
        {
            return new DbContext(Current, Transaction.Current, DbContextBehaviors.Default);
        }

        /// <summary>
        ///     Shares the ambient context if it exists; otherwise, creates a new context.
        /// </summary>
        /// <returns>a db context</returns>
        public static IDbContext SharedOrNewContext()
        {
            var ambient = DbContextFlowProvider.Peek();
            return (ambient != null)
                       ? ambient.ShareContext()
                       : new DbContext(null, Transaction.Current, DbContextBehaviors.Default);
        }
    }
}