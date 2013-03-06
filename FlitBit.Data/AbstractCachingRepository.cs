using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using FlitBit.Core;

namespace FlitBit.Data
{
	public abstract class AbstractCachingRepository<TModel, Id> : IDataRepository<TModel, Id>
	{
		protected static readonly string CCacheKey = typeof(TModel).AssemblyQualifiedName;
		readonly ConcurrentBag<AbstractCacheHandler> _cacheHandlers = new ConcurrentBag<AbstractCacheHandler>();
		readonly DbProviderHelper _helper;

		abstract class AbstractCacheHandler
		{
			internal abstract void PerformUpdateCacheItem(IDbContext context, TModel item);
			internal abstract void RemoveCacheItem(IDbContext context, TModel item);
		}
		class CacheHandler<TCacheKey, TItemKey> : AbstractCacheHandler
		{
			private TCacheKey _cacheKey;
			private Func<TModel, TItemKey> _calculateKey;

			public CacheHandler(TCacheKey cacheKey, Func<TModel, TItemKey> calculateKey)
			{
				this._cacheKey = cacheKey;
				this._calculateKey = calculateKey;
			}
			internal override void PerformUpdateCacheItem(IDbContext context, TModel item)
			{
				context.PutCacheItem(_cacheKey, item, _calculateKey(item), (k, v) => item);
			}
			internal override void RemoveCacheItem(IDbContext context, TModel item)
			{
				context.RemoveCacheItem(_cacheKey, item, _calculateKey(item));
			}
		}
		protected void PerformUpdateCacheItems(IDbContext context, IEnumerable<TModel> items)
		{
			foreach (var item in items)
			{
				foreach (var handler in _cacheHandlers)
				{
					handler.PerformUpdateCacheItem(context, item);
				}
			}
		}		
		protected void PerformUpdateCacheItem(IDbContext context, TModel item)
		{
			foreach (var handler in _cacheHandlers)
			{
				handler.PerformUpdateCacheItem(context, item);
			}
		}
		protected void PerformRemoveCacheItem(IDbContext context, TModel item)
		{
			foreach (var handler in _cacheHandlers)
			{
				handler.PerformUpdateCacheItem(context, item);
			}
		}

		protected void RegisterCacheHandler<TCacheKey, TKey>(TCacheKey cacheKey, Func<TModel, TKey> calculateKey)
		{
			_cacheHandlers.Add(new CacheHandler<TCacheKey, TKey>(cacheKey, calculateKey));
		}

		public AbstractCachingRepository(string connectionName)
		{
			Contract.Requires<ArgumentNullException>(connectionName != null);
			Contract.Requires<ArgumentException>(connectionName.Length > 0);

			this.ConnectionName = connectionName;
			this.RegisterCacheHandler(CCacheKey, GetIdentity);
			this._helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(connectionName);
		}

		protected TModel GetCacheItem<TCacheKey, TItemKey>(IDbContext context, TCacheKey cacheKey, TItemKey key, Func<IDbContext, TItemKey, TModel> resolver)
			where TCacheKey : class
		{
			TModel res;
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

		protected IEnumerable<TModel> GetCacheCollection<TCacheKey, TItemKey>(IDbContext context, TCacheKey cacheKey, TItemKey key, Func<IDbContext, TItemKey, IEnumerable<TModel>> resolver)
			where TCacheKey : class
		{
			IEnumerable<TModel> res;				
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
		
		protected string ConnectionName { get; private set; }

		protected DbProviderHelper Helper { get { return _helper; } }

		public abstract Id GetIdentity(TModel model);

		public TModel Create(IDbContext context, TModel model)
		{
			var res = PerformCreate(context, model);
			if (res != null)
			{
				bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
				if (!disableCaching) ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, res));
			}
			return res;
		}
		
		protected abstract TModel PerformCreate(IDbContext context, TModel model);

		public TModel Read(IDbContext context, Id id)
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

		protected abstract TModel PerformRead(IDbContext context, Id id);

		protected TModel ReadBy<TCacheKey, TItemKey>(IDbContext context, string command, Action<TItemKey, IDataParameterBinder> binder, TCacheKey cacheKey, TItemKey key)
			where TCacheKey : class
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);

			bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
			if (cacheKey != null && !disableCaching)
			{
				return GetCacheItem(context, cacheKey, key, (ctx, k) => PerformDirectReadBy(context, command, binder, key));
			}
			else
			{
				return PerformDirectReadBy(context, command, binder, key);
			}
		}

		protected abstract TModel PerformDirectReadBy<TItemKey>(IDbContext context, string command, Action<TItemKey, IDataParameterBinder> binder, TItemKey key);
		
		protected IEnumerable<TModel> QueryBy<TCacheKey, TQueryKey>(IDbContext context, string command, Action<TQueryKey, IDataParameterBinder> binder, TCacheKey cacheKey, TQueryKey key)
			where TCacheKey : class
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);

			bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
			if (cacheKey != null && !disableCaching)
			{
				return GetCacheCollection(context, cacheKey, key, (ctx, k) => PerformDirectQueryBy(context, command, binder, key));
			}
			else
			{
				return PerformDirectQueryBy(context, command, binder, key);
			}
		}

		protected abstract IEnumerable<TModel> PerformDirectQueryBy<TItemKey>(IDbContext context, string command, Action<TItemKey, IDataParameterBinder> binder, TItemKey key);

		public TModel Update(IDbContext context, TModel model)
		{
			var res = PerformUpdate(context, model);
			if (!context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, res));
			}
			return res;
		}

		protected abstract TModel PerformUpdate(IDbContext context, TModel model);

		public bool Delete(IDbContext context, Id id)
		{
			bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
			TModel cached = default(TModel);
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

		protected abstract bool PerformDelete(IDbContext context, Id id);

		public IEnumerable<TModel> All(IDbContext context, QueryBehavior behavior)
		{
			var res = PerformAll(context, behavior);

			if (!context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching) && !behavior.BypassCache)
			{
				ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItems(context, res));
			}
			return res;
		}

		protected abstract IEnumerable<TModel> PerformAll(IDbContext context, QueryBehavior behavior);

		public abstract IEnumerable<TModel> ReadMatch<TMatch>(IDbContext context, QueryBehavior behavior, TMatch match) where TMatch : class;

		public abstract int UpdateMatch<TMatch, TUpdate>(IDbContext context, TMatch match, TUpdate update)	where TMatch : class;

		public abstract int DeleteMatch<TMatch>(IDbContext context, TMatch match) where TMatch : class;

		public abstract IQueryable<TModel> Query();
	}

}
