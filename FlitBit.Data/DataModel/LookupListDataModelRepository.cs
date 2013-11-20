﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace FlitBit.Data.DataModel
{
  public abstract class LookupListDataModelRepository<TDataModel, TIdentityKey, TDbConnection>
    : IDataModelRepository<TDataModel, TIdentityKey, TDbConnection>
    where TDbConnection : DbConnection, new()
  {
    // ReSharper disable once StaticFieldInGenericType
    protected static readonly string CCacheKey = typeof(TDataModel).AssemblyQualifiedName;

    readonly ConcurrentDictionary<TIdentityKey, Tuple<TDataModel, DateTime>> _items =
      new ConcurrentDictionary<TIdentityKey, Tuple<TDataModel, DateTime>>();

    object _itemsInDefaultOrder;
    readonly DbProviderHelper _helper;
    readonly EqualityComparer<TDataModel> _comparer = EqualityComparer<TDataModel>.Default;
    readonly IDataModelBinder<TDataModel, TIdentityKey, TDbConnection> _binder;
    readonly IMapping<TDataModel> _mapping;
    TimeSpan _cacheRefreshSpan;

    protected LookupListDataModelRepository(IMapping<TDataModel> mapping)
      : this(mapping, TimeSpan.FromMinutes(5))
    {}

    protected LookupListDataModelRepository(IMapping<TDataModel> mapping, TimeSpan cacheRefreshSpan)
    {
      Contract.Requires<ArgumentNullException>(mapping != null);
      Contract.Requires<ArgumentException>(mapping.HasBinder);
      _mapping = mapping;
      _binder = (IDataModelBinder<TDataModel, TIdentityKey, TDbConnection>)mapping.GetBinder();
      ConnectionName = _mapping.ConnectionName;
      _helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(ConnectionName);
      _cacheRefreshSpan = cacheRefreshSpan;
    }

    protected string ConnectionName { get; private set; }

    protected DbProviderHelper Helper { get { return _helper; } }

    public TimeSpan CacheRefreshSpan
    {
      get { return _cacheRefreshSpan; }
      set
      {
        Contract.Requires<ArgumentException>(value.Milliseconds > 0);
        _cacheRefreshSpan = value;
      }
    }

    public abstract TIdentityKey GetIdentity(TDataModel model);

    public TDataModel Create(IDbContext context, TDataModel model)
    {
      var res = PerformCreate(context, model);
      ClearCollectionCache();
      return res;
    }

    void ClearCollectionCache() { Interlocked.Exchange(ref _itemsInDefaultOrder, null); }

    public TDataModel ReadByIdentity(IDbContext context, TIdentityKey key)
    {
      TDataModel res;
      if (context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching))
      {
        res = PerformRead(context, key);
        ThreadPool.QueueUserWorkItem(
                                     unused => _items.AddOrUpdate(
                                                                  key,
                                                                  k => Tuple.Create(res, DateTime.Now),
                                                                  (k, _) => Tuple.Create(res, DateTime.Now))
          );
        return res;
      }
      var time = DateTime.Now;
      Tuple<TDataModel, DateTime> item;
      if (_items.TryGetValue(key, out item)
          && time <= item.Item2.Add(_cacheRefreshSpan))
      {
        return item.Item1;
      }
      // Not cached... got to the db...
      res = PerformRead(context, key);
      if (!_comparer.Equals(res, default(TDataModel)))
      {
        // If it was in the db then our cache is out of sync...
        ClearCollectionCache();
        _items.AddOrUpdate(key, k => Tuple.Create(res, DateTime.Now), (k, _) => Tuple.Create(res, DateTime.Now));
      }
      return res;
    }

    public TDataModel Update(IDbContext context, TDataModel model)
    {
      var res = PerformUpdate(context, model);
      var key = GetIdentity(model);
      _items.AddOrUpdate(key, k => Tuple.Create(res, DateTime.Now), (k, _) => Tuple.Create(res, DateTime.Now));
      ClearCollectionCache();
      return res;
    }

    public bool Delete(IDbContext context, TIdentityKey id)
    {
      var res = PerformDelete(context, id);
      if (res)
      {
        Tuple<TDataModel, DateTime> item;
        _items.TryRemove(id, out item);
        ClearCollectionCache();
      }
      return res;
    }

    public IDataModelQueryResult<TDataModel> All(IDbContext context, QueryBehavior behavior)
    {
      IDataModelQueryResult<TDataModel> res;
      if (context.Behaviors.HasFlag(DbContextBehaviors.DisableCaching)
          || behavior.Behaviors != QueryBehaviors.Default)
      {
        res = PerformAll(context, behavior);
        if (res.Succeeded)
        {
          ThreadPool.QueueUserWorkItem(unused => UpdateCollectionCache(res));
        }
        return res;
      }

      var items = (TDataModel[])Thread.VolatileRead(ref _itemsInDefaultOrder);
      if (items != null)
      {
        return new DataModelQueryResult<TDataModel>(behavior, items);
      }
      res = PerformAll(context, behavior);
      if (res.Succeeded)
      {
        UpdateCollectionCache(res);
      }
      return res;
    }

    void UpdateCollectionCache(IDataModelQueryResult<TDataModel> res)
    {
      Contract.Requires<ArgumentException>(res.Succeeded);
      var items = res.Results.ToArray();
      if (res.Behaviors.Behaviors == QueryBehaviors.Default)
      {
        Interlocked.Exchange(ref _itemsInDefaultOrder, items);
        _items.Clear();
      }
      var timestamp = DateTime.Now;
      for (var i = 0; i < items.Length; ++i)
      {
        var item = items[i];
        _items.AddOrUpdate(GetIdentity(item),
                           k => Tuple.Create(item, timestamp),
                           (k, _) => Tuple.Create(item, timestamp)
          );
      }
    }

    public IDataModelBinder<TDataModel, TIdentityKey, TDbConnection> Binder { get { return _binder; } }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam> Where<TParam>(string queryKey,
                                                                                   Expression
                                                                                     <Func<TDataModel, TParam, bool>>
                                                                                     predicate)
    {
      return _binder.MakeQueryCommand<TParam>(queryKey).Where(predicate);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1> Where<TParam, TParam1>(string queryKey,
                                                                                                     Expression
                                                                                                       <
                                                                                                       Func
                                                                                                       <TDataModel,
                                                                                                       TParam, TParam1,
                                                                                                       bool>> predicate)
    {
      return _binder.MakeQueryCommand<TParam, TParam1>(queryKey).Where(predicate);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> Where<TParam, TParam1, TParam2>(
      string queryKey, Expression<Func<TDataModel, TParam, TParam1, TParam2, bool>> predicate)
    {
      return Binder.MakeQueryCommand<TParam, TParam1, TParam2>(queryKey).Where(predicate);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> Where
      <TParam, TParam1, TParam2, TParam3>(string queryKey,
                                          Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, bool>>
                                            predicate)
    {
      return Binder.MakeQueryCommand<TParam, TParam1, TParam2, TParam3>(queryKey).Where(predicate);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> Where
      <TParam, TParam1, TParam2, TParam3, TParam4>(string queryKey,
                                                   Expression
                                                     <Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, bool>
                                                     > predicate)
    {
      return Binder.MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4>(queryKey).Where(predicate);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> Where
      <TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(string queryKey,
                                                            Expression
                                                              <
                                                              Func
                                                              <TDataModel, TParam, TParam1, TParam2, TParam3, TParam4,
                                                              TParam5, bool>> predicate)
    {
      return Binder.MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(queryKey).Where(predicate);
    }

    public
      IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>
      Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(string queryKey,
                                                                          Expression
                                                                            <
                                                                            Func
                                                                            <TDataModel, TParam, TParam1, TParam2,
                                                                            TParam3, TParam4, TParam5, TParam6, bool>>
                                                                            predicate)
    {
      return
        Binder.MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(queryKey).Where(predicate);
    }

    public
      IDataModelQueryCommand
        <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> Where
      <TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(string queryKey,
                                                                              Expression
                                                                                <
                                                                                Func
                                                                                <TDataModel, TParam, TParam1, TParam2,
                                                                                TParam3, TParam4, TParam5, TParam6,
                                                                                TParam7, bool>> predicate)
    {
      return
        Binder.MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(queryKey)
              .Where(predicate);
    }

    public
      IDataModelQueryCommand
        <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>
      Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(string queryKey,
                                                                                            Expression
                                                                                              <
                                                                                              Func
                                                                                              <TDataModel, TParam,
                                                                                              TParam1, TParam2, TParam3,
                                                                                              TParam4, TParam5, TParam6,
                                                                                              TParam7, TParam8, bool>>
                                                                                              predicate)
    {
      return
        Binder.MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(queryKey)
              .Where(predicate);
    }

    public
      IDataModelQueryCommand
        <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8,
          TParam9> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(
      string queryKey,
      Expression
        <Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, bool>
        >
        predicate)
    {
      return
        Binder.MakeQueryCommand<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>
          (
           queryKey).Where(predicate);
    }

    public TDataModel ExecuteSingle<TParam>(
      IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam> cmd,
      IDbContext cx,
      TParam param)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteSingle(cx, cn, param);
    }

    public TDataModel ExecuteSingle<TParam, TParam1>(
      IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam, TParam1> cmd,
      IDbContext cx,
      TParam param, TParam1 param1)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteSingle(cx, cn, param, param1);
    }

    public TDataModel ExecuteSingle<TParam, TParam1, TParam2>(
      IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> cmd,
      IDbContext cx,
      TParam param, TParam1 param1, TParam2 param2)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteSingle(cx, cn, param, param1, param2);
    }

    public TDataModel ExecuteSingle<TParam, TParam1, TParam2, TParam3>(
      IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> cmd,
      IDbContext cx,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteSingle(cx, cn, param, param1, param2, param3);
    }

    public TDataModel ExecuteSingle<TParam, TParam1, TParam2, TParam3, TParam4>(
      IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> cmd,
      IDbContext cx,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteSingle(cx, cn, param, param1, param2, param3, param4);
    }

    public TDataModel ExecuteSingle<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(
      IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> cmd,
      IDbContext cx,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteSingle(cx, cn, param, param1, param2, param3, param4, param5);
    }

    public TDataModel ExecuteSingle<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
      IDataModelQuerySingleCommand
        <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> cmd,
      IDbContext cx,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteSingle(cx, cn, param, param1, param2, param3, param4, param5, param6);
    }

    public TDataModel ExecuteSingle<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
      IDataModelQuerySingleCommand
        <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> cmd,
      IDbContext cx,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6,
      TParam7 param7)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteSingle(cx, cn, param, param1, param2, param3, param4, param5, param6, param7);
    }

    public TDataModel ExecuteSingle<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
      IDataModelQuerySingleCommand
        <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> cmd,
      IDbContext cx,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6,
      TParam7 param7, TParam8 param8)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteSingle(cx, cn, param, param1, param2, param3, param4, param5, param6, param7, param8);
    }

    public TDataModel ExecuteSingle
      <TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(
      IDataModelQuerySingleCommand
        <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8,
        TParam9> cmd, IDbContext cx, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4,
      TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteSingle(cx, cn, param, param1, param2, param3, param4, param5, param6, param7, param8, param9);
    }

    public IDataModelQueryResult<TDataModel> ExecuteMany<TParam>(
      IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam> cmd,
      IDbContext cx, QueryBehavior behavior,
      TParam param)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteMany(cx, cn, behavior, param);
    }

    public IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1>(
      IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam, TParam1> cmd,
      IDbContext cx, QueryBehavior behavior,
      TParam param, TParam1 param1)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteMany(cx, cn, behavior, param, param1);
    }

    public IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1, TParam2>(
      IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> cmd,
      IDbContext cx, QueryBehavior behavior,
      TParam param, TParam1 param1, TParam2 param2)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteMany(cx, cn, behavior, param, param1, param2);
    }

    public IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1, TParam2, TParam3>(
      IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> cmd,
      IDbContext cx, QueryBehavior behavior,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteMany(cx, cn, behavior, param, param1, param2, param3);
    }

    public IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1, TParam2, TParam3, TParam4>(
      IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> cmd,
      IDbContext cx, QueryBehavior behavior,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteMany(cx, cn, behavior, param, param1, param2, param3, param4);
    }

    public IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(
      IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> cmd,
      IDbContext cx, QueryBehavior behavior,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteMany(cx, cn, behavior, param, param1, param2, param3, param4, param5);
    }

    public IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
      IDataModelQueryManyCommand
        <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> cmd,
      IDbContext cx, QueryBehavior behavior,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteMany(cx, cn, behavior, param, param1, param2, param3, param4, param5, param6);
    }

    public IDataModelQueryResult<TDataModel> ExecuteMany
      <TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
      IDataModelQueryManyCommand
        <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> cmd,
      IDbContext cx, QueryBehavior behavior,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6,
      TParam7 param7)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteMany(cx, cn, behavior, param, param1, param2, param3, param4, param5, param6, param7);
    }

    public IDataModelQueryResult<TDataModel> ExecuteMany
      <TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
      IDataModelQueryManyCommand
        <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> cmd,
      IDbContext cx, QueryBehavior behavior,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6,
      TParam7 param7, TParam8 param8)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteMany(cx, cn, behavior, param, param1, param2, param3, param4, param5, param6, param7, param8);
    }

    public IDataModelQueryResult<TDataModel> ExecuteMany
      <TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(
      IDataModelQueryManyCommand
        <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8,
        TParam9> cmd, IDbContext cx, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2,
      TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9)
    {
      var cn = cx.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteMany(cx, cn, behavior, param, param1, param2, param3, param4, param5, param6, param7, param8,
                             param9);
    }

    protected virtual IDataModelQueryResult<TDataModel> PerformAll(IDbContext context, QueryBehavior behavior)
    {
      var cmd = _binder.GetAllCommand();
      var cn = context.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }
      return cmd.ExecuteMany(context, cn, behavior);
    }

    protected virtual TDataModel PerformCreate(IDbContext context, TDataModel model)
    {
      var cmd = _binder.GetCreateCommand();
      var cn = context.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteSingle(context, cn, model);
    }

    protected virtual bool PerformDelete(IDbContext context, TIdentityKey id)
    {
      var cmd = _binder.GetDeleteCommand();
      var cn = context.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }
      return cmd.Execute(context, cn, id) == 1;
    }

    protected virtual TDataModel PerformRead(IDbContext context, TIdentityKey id)
    {
      var cmd = _binder.GetReadCommand();
      var cn = context.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }
      return cmd.ExecuteSingle(context, cn, id);
    }

    protected virtual TDataModel PerformUpdate(IDbContext context, TDataModel model)
    {
      var cmd = _binder.GetUpdateCommand();
      var cn = context.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }
      return cmd.ExecuteSingle(context, cn, model);
    }

    protected virtual IEnumerable<TDataModel> PerformDirectQueryBy<TItemKey>(IDbContext context, string command,
                                                                             Action<DbCommand, TItemKey> binder,
                                                                             TItemKey key)
    {
      throw new NotImplementedException();
    }

    protected virtual TDataModel PerformDirectReadBy<TItemKey>(IDbContext context, string command,
                                                               Action<DbCommand, TItemKey> binder, TItemKey key)
    {
      throw new NotImplementedException();
    }


    public IDataModelJoinCommandBuilder<TDataModel, TDbConnection, TJoin> Join<TJoin>(string queryKey, Expression<Func<TDataModel, TJoin, bool>> predicate)
    {
      return Binder.MakeJoinCommand<TJoin>(queryKey).Join(predicate);
    }
  }
}