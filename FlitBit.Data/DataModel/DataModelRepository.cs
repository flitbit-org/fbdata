using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading;
using FlitBit.Data.Expressions;
using FlitBit.Data.Repositories;

namespace FlitBit.Data.DataModel
{
  public abstract class DataModelRepository<TDataModel, TIdentityKey, TDbConnection>
    : AbstractCachingRepository<TDataModel, TIdentityKey>, IDataModelRepository<TDataModel, TIdentityKey, TDbConnection>
    where TDbConnection : DbConnection
  {
    readonly IDataModelBinder<TDataModel, TIdentityKey, TDbConnection> _binder;
    readonly IMapping<TDataModel> _mapping;

    protected DataModelRepository(IMapping<TDataModel> mapping)
      : base(mapping.ConnectionName)
    {
      Contract.Requires<ArgumentNullException>(mapping != null);
      Contract.Requires<ArgumentException>(mapping.HasBinder);
      _mapping = mapping;
      _binder = (IDataModelBinder<TDataModel, TIdentityKey, TDbConnection>)mapping.GetBinder();
    }

    public IDataModelBinder<TDataModel, TIdentityKey, TDbConnection> Binder { get { return _binder; } }

    public IDataModelWriter<TDataModel> Writer { get { return Binder.Writer; } }

    public IDataModelQueryBuilder<TDataModel, TIdentityKey, TDbConnection> QueryBuilder
    {
      get { return new DataModelQueryBuilder<TDataModel, TIdentityKey, TDbConnection>(this, Writer); }
    }

    public IDataModelQueryBuilder<TDataModel, TIdentityKey, TDbConnection> MakeNamedQueryBuilder(string name)
    {
      return new DataModelQueryBuilder<TDataModel, TIdentityKey, TDbConnection>(this, Writer, name);
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

    protected override IDataModelQueryResult<TDataModel> PerformAll(IDbContext context, QueryBehavior behavior)
    {
      var cmd = _binder.GetAllCommand();
      var cn = context.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }
      return cmd.ExecuteMany(context, cn, behavior);
    }

    protected override TDataModel PerformCreate(IDbContext context, TDataModel model)
    {
      var cmd = _binder.GetCreateCommand();
      var cn = context.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }

      return cmd.ExecuteSingle(context, cn, model);
    }

    protected override bool PerformDelete(IDbContext context, TIdentityKey id)
    {
      var cmd = _binder.GetDeleteCommand();
      var cn = context.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }
      return cmd.Execute(context, cn, id) == 1;
    }

    protected override TDataModel PerformRead(IDbContext context, TIdentityKey id)
    {
      var cmd = _binder.GetReadCommand();
      var cn = context.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }
      return cmd.ExecuteSingle(context, cn, id);
    }

    protected override TDataModel PerformUpdate(IDbContext context, TDataModel model)
    {
      var cmd = _binder.GetUpdateCommand();
      var cn = context.SharedOrNewConnection<TDbConnection>(ConnectionName);
      if (!cn.State.HasFlag(ConnectionState.Open))
      {
        cn.Open();
      }
      return cmd.ExecuteSingle(context, cn, model);
    }

    protected override IEnumerable<TDataModel> PerformDirectQueryBy<TItemKey>(IDbContext context, string command,
      Action<DbCommand, TItemKey> binder, TItemKey key)
    {
      throw new NotImplementedException();
    }

    protected override TDataModel PerformDirectReadBy<TItemKey>(IDbContext context, string command,
      Action<DbCommand, TItemKey> binder, TItemKey key)
    {
      throw new NotImplementedException();
    }

    public virtual object ConstructQueryCommand(string key, DataModelSqlExpression<TDataModel> sql)
    {
      return Binder.ConstructQueryCommand(this, key, sql, Writer);
    }
  }
}