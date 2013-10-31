using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using FlitBit.Data.DataModel;

namespace FlitBit.Data
{
	public abstract class AbstractCachingRepository<TModel, TIdentityKey> : IDataRepository<TModel, TIdentityKey>
	{
		protected static readonly string CCacheKey = typeof (TModel).AssemblyQualifiedName;
		private readonly ConcurrentBag<AbstractCacheHandler> _cacheHandlers = new ConcurrentBag<AbstractCacheHandler>();
		private readonly DbProviderHelper _helper;

		public AbstractCachingRepository(string connectionName)
		{
			Contract.Requires<ArgumentNullException>(connectionName != null);
			Contract.Requires<ArgumentException>(connectionName.Length > 0);

			ConnectionName = connectionName;
			RegisterCacheHandler(CCacheKey, GetIdentity);
			_helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(connectionName);
		}

		protected string ConnectionName { get; private set; }

		protected DbProviderHelper Helper
		{
			get { return _helper; }
		}

		protected abstract IDataModelQueryResult<TModel> PerformAll(IDbContext context, QueryBehavior behavior);

		protected abstract TModel PerformCreate(IDbContext context, TModel model);
		protected abstract bool PerformDelete(IDbContext context, TIdentityKey id);

		protected abstract IEnumerable<TModel> PerformDirectQueryBy<TItemKey>(IDbContext context, string command,
			Action<TItemKey, IDataParameterBinder> binder, TItemKey key);

		protected abstract TModel PerformDirectReadBy<TItemKey>(IDbContext context, string command,
			Action<TItemKey, IDataParameterBinder> binder, TItemKey key);

		protected abstract TModel PerformRead(IDbContext context, TIdentityKey id);

		protected abstract TModel PerformUpdate(IDbContext context, TModel model);

		protected IEnumerable<TModel> GetCacheCollection<TCacheKey, TItemKey>(IDbContext context, TCacheKey cacheKey, TItemKey key,
			Func<IDbContext, TItemKey, IEnumerable<TModel>> resolver)
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

		protected TModel GetCacheItem<TCacheKey, TItemKey>(IDbContext context, TCacheKey cacheKey, TItemKey key,
			Func<IDbContext, TItemKey, TModel> resolver)
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

		protected void PerformRemoveCacheItem(IDbContext context, TModel item)
		{
			foreach (AbstractCacheHandler handler in _cacheHandlers)
			{
				handler.PerformUpdateCacheItem(context, item);
			}
		}

		protected void PerformUpdateCacheItem(IDbContext context, TModel item)
		{
			foreach (AbstractCacheHandler handler in _cacheHandlers)
			{
				handler.PerformUpdateCacheItem(context, item);
			}
		}

		protected void PerformUpdateCacheItems(IDbContext context, IEnumerable<TModel> items)
		{
			foreach (TModel item in items)
			{
				foreach (AbstractCacheHandler handler in _cacheHandlers)
				{
					handler.PerformUpdateCacheItem(context, item);
				}
			}
		}

		protected IEnumerable<TModel> QueryBy<TCacheKey, TQueryKey>(IDbContext context, string command,
			Action<TQueryKey, IDataParameterBinder> binder, TCacheKey cacheKey, TQueryKey key)
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
			return PerformDirectQueryBy(context, command, binder, key);
		}

		protected TModel ReadBy<TCacheKey, TItemKey>(IDbContext context, string command,
			Action<TItemKey, IDataParameterBinder> binder, TCacheKey cacheKey, TItemKey key)
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
			return PerformDirectReadBy(context, command, binder, key);
		}

		protected void RegisterCacheHandler<TCacheKey, TKey>(TCacheKey cacheKey, Func<TModel, TKey> calculateKey)
		{
			_cacheHandlers.Add(new CacheHandler<TCacheKey, TKey>(cacheKey, calculateKey));
		}

		#region IDataRepository<M,IK> Members

		public abstract TIdentityKey GetIdentity(TModel model);

		public TModel Create(IDbContext context, TModel model)
		{
			TModel res = PerformCreate(context, model);
			if (res != null)
			{
				bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
				if (!disableCaching)
				{
					ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, res));
				}
			}
			return res;
		}

		public TModel ReadByIdentity(IDbContext context, TIdentityKey id)
		{
			if (context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				return PerformRead(context, id);
			}
			return GetCacheItem(context, CCacheKey, id, PerformRead);
		}

		public TModel Update(IDbContext context, TModel model)
		{
			TModel res = PerformUpdate(context, model);
			if (!context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItem(context, res));
			}
			return res;
		}

		public bool Delete(IDbContext context, TIdentityKey id)
		{
			bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
			TModel cached = default(TModel);
			if (!disableCaching)
			{
				context.TryGetCacheItem(CCacheKey, id, out cached);
			}
			bool res = PerformDelete(context, id);
			if (res && cached != null)
			{
				PerformRemoveCacheItem(context, cached);
			}
			return res;
		}

		public IDataModelQueryResult<TModel> All(IDbContext context, QueryBehavior behavior)
		{
			IDataModelQueryResult<TModel> res = PerformAll(context, behavior);

			if (res.Succeeded && !context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching) && !behavior.BypassCache)
			{
				ThreadPool.QueueUserWorkItem(unused => PerformUpdateCacheItems(context, res.Results));
			}
			return res;
		}

		#endregion

		private abstract class AbstractCacheHandler
		{
			internal abstract void PerformUpdateCacheItem(IDbContext context, TModel item);
			internal abstract void RemoveCacheItem(IDbContext context, TModel item);
		}

		private class CacheHandler<TCacheKey, TItemKey> : AbstractCacheHandler
		{
			private readonly TCacheKey _cacheKey;
			private readonly Func<TModel, TItemKey> _calculateKey;

			public CacheHandler(TCacheKey cacheKey, Func<TModel, TItemKey> calculateKey)
			{
				_cacheKey = cacheKey;
				_calculateKey = calculateKey;
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
	}
}