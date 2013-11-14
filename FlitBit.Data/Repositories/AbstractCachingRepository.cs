using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.Repositories
{
	public abstract class AbstractCachingRepository<TModel, TIdentityKey> : IDataRepository<TModel, TIdentityKey>
	{
		// ReSharper disable once StaticFieldInGenericType
		protected static readonly string CCacheKey = typeof(TModel).AssemblyQualifiedName;

		private readonly ConcurrentBag<AbstractCacheHandler<TModel>> _cacheHandlers = new ConcurrentBag<AbstractCacheHandler<TModel>>();
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
				Action<DbCommand, TItemKey> binder, TItemKey key);

		protected abstract TModel PerformDirectReadBy<TItemKey>(IDbContext context, string command,
				Action<DbCommand, TItemKey> binder, TItemKey key);

		protected abstract TModel PerformRead(IDbContext context, TIdentityKey id);

		protected abstract TModel PerformUpdate(IDbContext context, TModel model);

		protected IEnumerable<TModel> GetCacheCollection<TCacheKey, TItemKey>(IDbContext context, TCacheKey cacheKey,
				TItemKey key,
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
			var arr = res as TModel[] ?? res.ToArray();
			if (res != null)
			{
				PerformUpdateCacheItems(context, arr);
			}
			return arr;
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
				PerformUpdateCacheItem(context, res);
			}
			return res;
		}

		protected void PerformRemoveCacheItem(IDbContext context, TModel item)
		{
			foreach (var handler in _cacheHandlers)
			{
				handler.PerformUpdateCacheItem(context, item);
			}
		}

		protected void PerformUpdateCacheItem(IDbContext context, TModel item)
		{
			foreach (var handler in _cacheHandlers)
			{
				handler.PerformUpdateCacheItem(context, item);
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

		protected IEnumerable<TModel> QueryBy<TCacheKey, TQueryKey>(IDbContext context, string command,
				Action<DbCommand, TQueryKey> binder, TCacheKey cacheKey, TQueryKey key)
				where TCacheKey : class
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);

			bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
			if (cacheKey != null && !disableCaching)
			{
				return GetCacheCollection(context, cacheKey, key,
						(ctx, k) => PerformDirectQueryBy(context, command, binder, key));
			}
			return PerformDirectQueryBy(context, command, binder, key);
		}

		protected TModel ReadBy<TCacheKey, TItemKey>(IDbContext context, string command,
				Action<DbCommand, TItemKey> binder, TCacheKey cacheKey, TItemKey key)
				where TCacheKey : class
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);

			bool disableCaching = context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching);
			if (cacheKey != null && !disableCaching)
			{
				return GetCacheItem(context, cacheKey, key,
						(ctx, k) => PerformDirectReadBy(context, command, binder, key));
			}
			return PerformDirectReadBy(context, command, binder, key);
		}

		protected void RegisterCacheHandler<TCacheKey, TKey>(TCacheKey cacheKey, Func<TModel, TKey> calculateKey)
		{
			AddCacheHandler(new ContextCacheByIdentityKeyCacheHandler<TModel, TCacheKey, TKey>(cacheKey, calculateKey));
		}

		protected void AddCacheHandler(AbstractCacheHandler<TModel> handler)
		{
			Contract.Requires<ArgumentNullException>(handler != null);
			_cacheHandlers.Add(handler);
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
					PerformUpdateCacheItem(context, res);
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
				PerformUpdateCacheItem(context, res);
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
				PerformUpdateCacheItems(context, res.Results);
			}
			return res;
		}

		#endregion

	}
}