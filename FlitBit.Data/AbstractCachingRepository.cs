using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using FlitBit.Data.DataModel;

namespace FlitBit.Data
{
	public abstract class AbstractCachingRepository<M, IK> : IDataRepository<M, IK>
	{
		protected static readonly string CCacheKey = typeof(M).AssemblyQualifiedName;
		readonly ConcurrentBag<AbstractCacheHandler> _cacheHandlers = new ConcurrentBag<AbstractCacheHandler>();
		readonly DbProviderHelper _helper;

		public AbstractCachingRepository(string connectionName)
		{
			Contract.Requires<ArgumentNullException>(connectionName != null);
			Contract.Requires<ArgumentException>(connectionName.Length > 0);

			this.ConnectionName = connectionName;
			this.RegisterCacheHandler(CCacheKey, GetIdentity);
			this._helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(connectionName);
		}

		protected string ConnectionName { get; private set; }

		protected DbProviderHelper Helper { get { return _helper; } }

		protected abstract IDataModelQueryResult<M> PerformAll(IDbContext context, QueryBehavior behavior);

		protected abstract M PerformCreate(IDbContext context, M model);
		protected abstract bool PerformDelete(IDbContext context, IK id);

		protected abstract IEnumerable<M> PerformDirectQueryBy<TItemKey>(IDbContext context, string command,
			Action<TItemKey, IDataParameterBinder> binder, TItemKey key);

		protected abstract M PerformDirectReadBy<TItemKey>(IDbContext context, string command,
			Action<TItemKey, IDataParameterBinder> binder, TItemKey key);

		protected abstract M PerformRead(IDbContext context, IK id);

		protected abstract M PerformUpdate(IDbContext context, M model);

		protected IEnumerable<M> GetCacheCollection<TCacheKey, TItemKey>(IDbContext context, TCacheKey cacheKey, TItemKey key,
			Func<IDbContext, TItemKey, IEnumerable<M>> resolver)
			where TCacheKey : class
		{
			IEnumerable<M> res;
			if (cacheKey != null)
			{
				if (context.TryGetCacheItem(cacheKey, key, out res))
				{
					return res;
				}
			}
			res = resolver(context, key);
			if (res != null)
			{
				ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItems(context, res));
			}
			return res;
		}

		protected M GetCacheItem<TCacheKey, TItemKey>(IDbContext context, TCacheKey cacheKey, TItemKey key,
			Func<IDbContext, TItemKey, M> resolver)
			where TCacheKey : class
		{
			M res;
			if (cacheKey != null)
			{
				if (context.TryGetCacheItem(cacheKey, key, out res))
				{
					return res;
				}
			}
			res = resolver(context, key);
			if (res != null)
			{
				ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, res));
			}
			return res;
		}

		protected void PerformRemoveCacheItem(IDbContext context, M item)
		{
			foreach (var handler in _cacheHandlers)
			{
				handler.PerformUpdateCacheItem(context, item);
			}
		}

		protected void PerformUpdateCacheItem(IDbContext context, M item)
		{
			foreach (var handler in _cacheHandlers)
			{
				handler.PerformUpdateCacheItem(context, item);
			}
		}

		protected void PerformUpdateCacheItems(IDbContext context, IEnumerable<M> items)
		{
			foreach (var item in items)
			{
				foreach (var handler in _cacheHandlers)
				{
					handler.PerformUpdateCacheItem(context, item);
				}
			}
		}

		protected IEnumerable<M> QueryBy<TCacheKey, TQueryKey>(IDbContext context, string command,
			Action<TQueryKey, IDataParameterBinder> binder, TCacheKey cacheKey, TQueryKey key)
			where TCacheKey : class
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);

			var disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
			if (cacheKey != null && !disableCaching)
			{
				return GetCacheCollection(context, cacheKey, key, (ctx, k) => PerformDirectQueryBy(context, command, binder, key));
			}
			else
			{
				return PerformDirectQueryBy(context, command, binder, key);
			}
		}

		protected M ReadBy<TCacheKey, TItemKey>(IDbContext context, string command,
			Action<TItemKey, IDataParameterBinder> binder, TCacheKey cacheKey, TItemKey key)
			where TCacheKey : class
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);

			var disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
			if (cacheKey != null && !disableCaching)
			{
				return GetCacheItem(context, cacheKey, key, (ctx, k) => PerformDirectReadBy(context, command, binder, key));
			}
			else
			{
				return PerformDirectReadBy(context, command, binder, key);
			}
		}

		protected void RegisterCacheHandler<TCacheKey, TKey>(TCacheKey cacheKey, Func<M, TKey> calculateKey)
		{
			_cacheHandlers.Add(new CacheHandler<TCacheKey, TKey>(cacheKey, calculateKey));
		}

		#region IDataRepository<M,IK> Members

		public abstract IK GetIdentity(M model);

		public M Create(IDbContext context, M model)
		{
			var res = PerformCreate(context, model);
			if (res != null)
			{
				var disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
				if (!disableCaching)
				{
					ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, res));
				}
			}
			return res;
		}

		public M Read(IDbContext context, IK id)
		{
			if (context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				return PerformRead(context, id);
			}
			else
			{
				return GetCacheItem(context, CCacheKey, id, PerformRead);
			}
		}

		public M Update(IDbContext context, M model)
		{
			var res = PerformUpdate(context, model);
			if (!context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, res));
			}
			return res;
		}

		public bool Delete(IDbContext context, IK id)
		{
			var disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
			var cached = default(M);
			if (!disableCaching)
			{
				context.TryGetCacheItem(CCacheKey, id, out cached);
			}
			var res = PerformDelete(context, id);
			if (res && cached != null)
			{
				PerformRemoveCacheItem(context, cached);
			}
			return res;
		}

		public IDataModelQueryResult<M> All(IDbContext context, QueryBehavior behavior)
		{
			var res = PerformAll(context, behavior);

			if (res.Succeeded && !context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching) && !behavior.BypassCache)
			{
				ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItems(context, res.Results));
			}
			return res;
		}

		public abstract IDataModelQueryResult<M> ReadMatch<TMatch>(IDbContext context, QueryBehavior behavior, TMatch match)
			where TMatch : class;

		public abstract int UpdateMatch<TMatch, TUpdate>(IDbContext context, TMatch match, TUpdate update)
			where TMatch : class
			where TUpdate : class;

		public abstract int DeleteMatch<TMatch>(IDbContext context, TMatch match) where TMatch : class;

		public abstract IQueryable<M> Query();

		#endregion

		abstract class AbstractCacheHandler
		{
			internal abstract void PerformUpdateCacheItem(IDbContext context, M item);
			internal abstract void RemoveCacheItem(IDbContext context, M item);
		}

		class CacheHandler<TCacheKey, TItemKey> : AbstractCacheHandler
		{
			TCacheKey _cacheKey;
			Func<M, TItemKey> _calculateKey;

			public CacheHandler(TCacheKey cacheKey, Func<M, TItemKey> calculateKey)
			{
				this._cacheKey = cacheKey;
				this._calculateKey = calculateKey;
			}

			internal override void PerformUpdateCacheItem(IDbContext context, M item)
			{
				context.PutCacheItem(_cacheKey, item, _calculateKey(item), (k, v) => item);
			}

			internal override void RemoveCacheItem(IDbContext context, M item)
			{
				context.RemoveCacheItem(_cacheKey, item, _calculateKey(item));
			}
		}
	}
}