using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.Repositories
{
	public abstract class AbstractCachingRepository<TModel, TIdentityKey> : IDataRepository<TModel, TIdentityKey>
	{
		// ReSharper disable once StaticFieldInGenericType
		protected static readonly string CCacheKey = typeof(TModel).AssemblyQualifiedName;
		// ReSharper disable once StaticFieldInGenericType
		protected static readonly string CCacheKeyAll = CCacheKey + "All Items";


		private readonly DbProviderHelper _helper;
		private readonly EqualityComparer<TModel> _comparer = EqualityComparer<TModel>.Default;

		public AbstractCachingRepository(string connectionName)
		{
			Contract.Requires<ArgumentNullException>(connectionName != null);
			Contract.Requires<ArgumentException>(connectionName.Length > 0);

			ConnectionName = connectionName;
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

		protected IEnumerable<TModel> QueryBy<TCacheKey, TQueryKey>(IDbContext context, string command,
			Action<DbCommand, TQueryKey> binder, TCacheKey cacheKey, TQueryKey key)
			where TCacheKey : class
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);

			if (context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				return PerformDirectQueryBy(context, command, binder, key);
			}
			var cache = GetContextCollectionCache(context, cacheKey, key);
			var res = cache.Get(key);
			if (res == null)
			{
				res = PerformDirectQueryBy(context, command, binder, key);
				if (res != null)
				{
					cache.Put(key, res);
				}
			}
			return res;
		}

		protected TModel ReadBy<TCacheKey, TItemKey>(IDbContext context, string command,
			Action<DbCommand, TItemKey> binder, TCacheKey cacheKey, TItemKey key)
			where TCacheKey : class
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);

			if (context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				return PerformDirectReadBy(context, command, binder, key);
			}
			var cache = GetContextCache(context, cacheKey, key);
			TModel res = cache.Get(key);
			if (_comparer.Equals(default(TModel), res))
			{
				res = PerformDirectReadBy(context, command, binder, key);
				if (!_comparer.Equals(default(TModel), res))
				{
					cache.Put(key, res);
				}
			}
			return res;
		}

		protected virtual ContextCache<TCacheKey, TKey, TModel> GetContextCache<TCacheKey, TKey>(IDbContext context, TCacheKey cacheKey,
			TKey key)
			where TCacheKey : class
		{
			return context.EnsureCache(cacheKey, MakeContextCache<TCacheKey, TKey>);
		}

		protected virtual ContextCache<TCacheKey, TKey, TModel> MakeContextCache<TCacheKey, TKey>(IDbContext context, TCacheKey cacheKey)
			where TCacheKey : class
		{
			return new SimpleContextCache<TCacheKey, TKey, TModel>(cacheKey, HandleContextCacheEnd, context);
		}

		protected virtual void HandleContextCacheEnd<TCacheKey, TKey>(IDbContext cx, TCacheKey cacheKey,
			IEnumerable<KeyValuePair<TKey, TModel>> items)
		{
		}

		protected virtual ContextCache<TCacheKey, TKey, IEnumerable<TModel>> GetContextCollectionCache<TCacheKey, TKey>(IDbContext context, TCacheKey cacheKey,
			TKey key)
			where TCacheKey : class
		{
			return context.EnsureCache(cacheKey, MakeContextCollectionCache<TCacheKey, TKey>);
		}

		protected virtual ContextCache<TCacheKey, TKey, IEnumerable<TModel>> MakeContextCollectionCache<TCacheKey, TKey>(IDbContext context, TCacheKey cacheKey)
			where TCacheKey : class
		{
			return new SimpleContextCache<TCacheKey, TKey, IEnumerable<TModel>>(cacheKey, HandleContextCollectionCacheEnd, context);
		}

		protected virtual void HandleContextCollectionCacheEnd<TCacheKey, TKey>(IDbContext cx, TCacheKey cacheKey,
		IEnumerable<KeyValuePair<TKey, IEnumerable<TModel>>> items)
		{
		}

		#region IDataRepository<M,IK> Members

		public abstract TIdentityKey GetIdentity(TModel model);

		public TModel Create(IDbContext context, TModel model)
		{
			if (context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				return PerformCreate(context, model);
			}
			var res = PerformCreate(context, model);
			var id = GetIdentity(model);
			var cache = GetContextCache(context, CCacheKey, id);
			cache.Put(id, res);
			return res;
		}

		public TModel ReadByIdentity(IDbContext context, TIdentityKey key)
		{
			if (context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				return PerformRead(context, key);
			}
			var cache = GetContextCache(context, CCacheKey, key);
			var res = cache.Get(key);
			if (_comparer.Equals(default(TModel), res))
			{
				res = PerformRead(context, key);
				if (!_comparer.Equals(default(TModel), res))
				{
					cache.Put(key, res);
				}
			}
			return res;
		}

		public TModel Update(IDbContext context, TModel model)
		{
			if (context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				return PerformUpdate(context, model);
			}
			var res = PerformUpdate(context, model);
			var id = GetIdentity(model);
			var cache = GetContextCache(context, CCacheKey, id);
			cache.Put(id, res);
			return res;
		}

		public bool Delete(IDbContext context, TIdentityKey id)
		{
			if (context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
			{
				return PerformDelete(context, id);
			}
			var res = PerformDelete(context, id);
			var cache = GetContextCache(context, CCacheKey, id);
			cache.Remove(id);
			return res;
		}

		public IDataModelQueryResult<TModel> All(IDbContext context, QueryBehavior behavior)
		{
			return PerformAll(context, behavior);

		}

		#endregion

	}
}