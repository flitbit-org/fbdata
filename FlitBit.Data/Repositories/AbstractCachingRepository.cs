using FlitBit.Core;
using FlitBit.Data.Cluster;
using FlitBit.Data.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Repositories
{
  public abstract class AbstractCachingRepository<TModel, TIdentityKey> : IDataRepository<TModel, TIdentityKey>,
    IClusterNotificationHandler, ICachePromotionHandler
  {
    readonly EqualityComparer<TModel> _comparer = EqualityComparer<TModel>.Default;
    readonly DbProviderHelper _helper;
    readonly TimeSpan _cacheTtl;
    readonly string _modelMemoryKeyPrefix;
    readonly IClusteredMemory _mem;
    ClusterObserver _observer;

    protected AbstractCachingRepository(string connectionName)
      : this(connectionName, TimeSpan.FromMinutes(5))
    {}

    protected AbstractCachingRepository(string connectionName, TimeSpan cacheTtl)
      : this(connectionName, cacheTtl, null)
    {}

    protected AbstractCachingRepository(string connectionName, TimeSpan cacheTtl, IClusteredMemory mem)
    {
      Contract.Requires<ArgumentNullException>(connectionName != null);
      Contract.Requires<ArgumentException>(connectionName.Length > 0);

      ConnectionName = connectionName;
      _helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(connectionName);
      _cacheTtl = cacheTtl;
      _modelMemoryKeyPrefix = typeof(TModel).GetReadableFullName();
      _mem = mem;
      if (mem == null)
      {
        if (FactoryProvider.Factory.CanConstruct<IClusteredMemory>())
        {
          _mem = FactoryProvider.Factory.CreateInstance<IClusteredMemory>();
        }
      }
    }

    public IClusteredMemory ClusterMemory { get { return _mem; } }

    protected bool SubscribeClusterNotification(string channel, IEnumerable<string> observations)
    {
      var notif = ClusterNotifications.Instance;
      if (notif != null)
      {
        _observer = new ClusterObserver(this, channel, observations);
        notif.Subscribe(_observer);
        return true;
      }
      return false;
    }

    protected void UnsubscribeClusterNotification()
    {
      if (_observer != null)
      {
        ClusterNotifications.Instance.Cancel(_observer.Key);
      }
    }

    protected virtual string IdToString(TIdentityKey id) { return Convert.ToString(id); }

    protected virtual TIdentityKey IdFromString(string id)
    {
      return (TIdentityKey)Convert.ChangeType(id, typeof(TIdentityKey));
    }

    protected string ConnectionName { get; private set; }

    protected DbProviderHelper Helper { get { return _helper; } }

    protected abstract IDataModelQueryResult<TModel> PerformAll(IDbContext context, QueryBehavior behavior);

    protected abstract TModel PerformCreate(IDbContext context, TModel model);

    protected abstract bool PerformDelete(IDbContext context, TIdentityKey id);

    protected abstract IEnumerable<TModel> PerformDirectQueryBy<TItemKey>(IDbContext context, string command,
      Action<DbCommand, TItemKey> binder, TItemKey key);

    protected abstract TModel PerformDirectReadBy<TItemKey>(IDbContext context, string command,
      Action<DbCommand, TItemKey> binder, TItemKey key);

    protected abstract TModel PerformRead(IDbContext context, TIdentityKey id);

    protected abstract TModel PerformUpdate(IDbContext context, TModel model);

    protected virtual string FormatClusteredMemoryKey(string key)
    {
      return String.Concat(_modelMemoryKeyPrefix, ":", key);
    }

    protected IEnumerable<TModel> QueryBy<TQueryKey>(IDbContext context, string command,
      Action<DbCommand, TQueryKey> binder, TQueryKey key, string cacheKey)
    {
      Contract.Requires<ArgumentNullException>(context != null);
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentException>(command.Length > 0);

      return PerformDirectQueryBy(context, command, binder, key);
    }

    protected TModel ReadBy<TItemKey>(IDbContext context, string command,
      Action<DbCommand, TItemKey> binder, TItemKey key, string cacheKey)
    {
      Contract.Requires<ArgumentNullException>(context != null);
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentException>(command.Length > 0);

      return PerformDirectReadBy(context, command, binder, key);
    }

    public abstract TIdentityKey GetIdentity(TModel model);

    public TModel Create(IDbContext context, TModel model)
    {
      var res = PerformCreate(context, model);
      CachePut(context, res, true);
      return res;
    }

    public TModel ReadByIdentity(IDbContext context, TIdentityKey key)
    {
      TModel res;
      if (!TryCacheRead(context, key, out res))
      {
        res = PerformRead(context, key);
        if (!_comparer.Equals(res, default(TModel)))
        {
          CachePut(context, res, false);
        }
      }
      return res;
    }

    private void CachePut(IDbContext ctx, TModel item, bool created)
    {
      if (!ctx.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
      {
        var id = GetIdentity(item);
        PerformCachePut(ctx, id, item, created);
      }
    }

    protected virtual void PerformCachePut(IDbContext ctx, TIdentityKey id, TModel item, bool created)
    {
      var key = FormatClusteredMemoryKey(IdToString(id));
      ctx.PutWithExpiration(key, item, created, _cacheTtl, this);
    }

    protected bool TryCacheRead(IDbContext ctx, TIdentityKey key, out TModel item)
    {
      if (!ctx.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
      {
        return PerformTryCacheRead(ctx, key, out item);
      }
      item = default(TModel);
      return false;
    }

    protected virtual bool PerformTryCacheRead(IDbContext ctx, TIdentityKey key, out TModel res)
    {
      var cacheKey = FormatClusteredMemoryKey(IdToString(key));
      return ctx.TryGet(_mem, cacheKey, out res);
    }

    public TModel Update(IDbContext context, TModel model)
    {
      var res = PerformUpdate(context, model);
      CachePut(context, res, false);
      return res;
    }

    public bool Delete(IDbContext context, TIdentityKey key)
    {
      var res = PerformDelete(context, key);
      if (res)
      {
        CacheDelete(context, key);
      }
      return res;
    }

    private void CacheDelete(IDbContext ctx, TIdentityKey key)
    {
      if (!ctx.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
      {
        PerformCacheDelete(ctx, key);
      }
    }

    protected virtual void PerformCacheDelete(IDbContext ctx, TIdentityKey key)
    {
      var cacheKey = FormatClusteredMemoryKey(IdToString(key));
      ctx.Delete(cacheKey, this);
    }

    public IDataModelQueryResult<TModel> All(IDbContext context, QueryBehavior behavior)
    {
      return PerformAll(context, behavior);
    }
    
    public virtual void ClusterNotify(string observation, string identity, IClusteredMemory mem) { }
    public virtual void PromoteCacheItem<T>(string key, TimeSpan ttl, T item, bool created) { }
    public virtual void PromoteCacheItem<T>(string key, T item, bool created) { }
    public virtual void PromoteCacheDeletion(string key) { }
  }
}