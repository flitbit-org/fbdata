using System;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace FlitBit.Data.DataModel
{
  public static class IDataModelRepositoryExtensions
  {

    public static IDataModelQueryResult<TDataModel> ExecuteMany<TDataModel, TIdentityKey, TDbConnection,
                                                                TParam>(
      this IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> repo,
      IDataModelQueryManyCommand<TDataModel, TDbConnection,
        TParam> cmd,
      TParam param)
      where TDbConnection : DbConnection
    {
      Contract.Requires<ArgumentNullException>(repo != null);
      Contract.Requires<ArgumentNullException>(cmd != null);
      Contract.Ensures(Contract.Result<IDataModelQueryResult<TDataModel>>() != null);
      return repo.ExecuteMany(cmd, DbContext.Current, QueryBehavior.Default, param);
    }
    public static IDataModelQueryResult<TDataModel> ExecuteMany<TDataModel, TIdentityKey, TDbConnection,
                                                                TParam, TParam1>(
      this IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> repo,
      IDataModelQueryManyCommand<TDataModel, TDbConnection,
        TParam, TParam1> cmd,
      TParam param, TParam1 param1)
      where TDbConnection : DbConnection
    {
      Contract.Requires<ArgumentNullException>(repo != null);
      Contract.Requires<ArgumentNullException>(cmd != null);
      Contract.Ensures(Contract.Result<IDataModelQueryResult<TDataModel>>() != null);
      return repo.ExecuteMany(cmd, DbContext.Current, QueryBehavior.Default, param, param1);
    }
    public static IDataModelQueryResult<TDataModel> ExecuteMany<TDataModel, TIdentityKey, TDbConnection,
                                                                TParam, TParam1, TParam2>(
      this IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> repo,
      IDataModelQueryManyCommand<TDataModel, TDbConnection,
        TParam, TParam1, TParam2> cmd,
      TParam param, TParam1 param1, TParam2 param2)
      where TDbConnection : DbConnection
    {
      Contract.Requires<ArgumentNullException>(repo != null);
      Contract.Requires<ArgumentNullException>(cmd != null);
      Contract.Ensures(Contract.Result<IDataModelQueryResult<TDataModel>>() != null);
      return repo.ExecuteMany(cmd, DbContext.Current, QueryBehavior.Default, param, param1, param2);
    }
    public static IDataModelQueryResult<TDataModel> ExecuteMany<TDataModel, TIdentityKey, TDbConnection,
                                                                TParam, TParam1, TParam2, TParam3>(
      this IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> repo,
      IDataModelQueryManyCommand<TDataModel, TDbConnection,
        TParam, TParam1, TParam2, TParam3> cmd,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3)
      where TDbConnection : DbConnection
    {
      Contract.Requires<ArgumentNullException>(repo != null);
      Contract.Requires<ArgumentNullException>(cmd != null);
      Contract.Ensures(Contract.Result<IDataModelQueryResult<TDataModel>>() != null);
      return repo.ExecuteMany(cmd, DbContext.Current, QueryBehavior.Default, param, param1, param2, param3);
    }

    public static IDataModelQueryResult<TDataModel> ExecuteMany<TDataModel, TIdentityKey, TDbConnection,
                                                               TParam, TParam1, TParam2, TParam3, TParam4>(
     this IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> repo,
     IDataModelQueryManyCommand<TDataModel, TDbConnection,
       TParam, TParam1, TParam2, TParam3, TParam4> cmd,
     TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
     where TDbConnection : DbConnection
    {
      Contract.Requires<ArgumentNullException>(repo != null);
      Contract.Requires<ArgumentNullException>(cmd != null);
      Contract.Ensures(Contract.Result<IDataModelQueryResult<TDataModel>>() != null);
      return repo.ExecuteMany(cmd, DbContext.Current, QueryBehavior.Default, param, param1, param2, param3, param4);
    }
    public static IDataModelQueryResult<TDataModel> ExecuteMany<TDataModel, TIdentityKey, TDbConnection,
                                                               TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(
     this IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> repo,
     IDataModelQueryManyCommand<TDataModel, TDbConnection,
       TParam, TParam1, TParam2, TParam3, TParam4, TParam5> cmd,
     TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
     where TDbConnection : DbConnection
    {
      Contract.Requires<ArgumentNullException>(repo != null);
      Contract.Requires<ArgumentNullException>(cmd != null);
      Contract.Ensures(Contract.Result<IDataModelQueryResult<TDataModel>>() != null);
      return repo.ExecuteMany(cmd, DbContext.Current, QueryBehavior.Default, param, param1, param2, param3, param4,
        param5);
    }
    public static IDataModelQueryResult<TDataModel> ExecuteMany<TDataModel, TIdentityKey, TDbConnection,
                                                               TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
     this IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> repo,
     IDataModelQueryManyCommand<TDataModel, TDbConnection,
       TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> cmd,
     TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6)
     where TDbConnection : DbConnection
    {
      Contract.Requires<ArgumentNullException>(repo != null);
      Contract.Requires<ArgumentNullException>(cmd != null);
      Contract.Ensures(Contract.Result<IDataModelQueryResult<TDataModel>>() != null);
      return repo.ExecuteMany(cmd, DbContext.Current, QueryBehavior.Default, param, param1, param2, param3, param4,
        param5, param6);
    }
    public static IDataModelQueryResult<TDataModel> ExecuteMany<TDataModel, TIdentityKey, TDbConnection,
                                                               TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
     this IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> repo,
     IDataModelQueryManyCommand<TDataModel, TDbConnection,
       TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> cmd,
     TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6,
     TParam7 param7)
     where TDbConnection : DbConnection
    {
      Contract.Requires<ArgumentNullException>(repo != null);
      Contract.Requires<ArgumentNullException>(cmd != null);
      Contract.Ensures(Contract.Result<IDataModelQueryResult<TDataModel>>() != null);
      return repo.ExecuteMany(cmd, DbContext.Current, QueryBehavior.Default, param, param1, param2, param3, param4,
        param5, param6, param7);
    }
   
    public static IDataModelQueryResult<TDataModel> ExecuteMany<TDataModel, TIdentityKey, TDbConnection,
                                                                TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
      this IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> repo,
      IDataModelQueryManyCommand<TDataModel, TDbConnection,
        TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> cmd,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6,
      TParam7 param7, TParam8 param8)
      where TDbConnection : DbConnection
    {
      Contract.Requires<ArgumentNullException>(repo != null);
      Contract.Requires<ArgumentNullException>(cmd != null);
      Contract.Ensures(Contract.Result<IDataModelQueryResult<TDataModel>>() != null);
      return repo.ExecuteMany(cmd, DbContext.Current, QueryBehavior.Default, param, param1, param2, param3, param4,
        param5, param6, param7, param8);
    }
    public static IDataModelQueryResult<TDataModel> ExecuteMany<TDataModel, TIdentityKey, TDbConnection, 
                                                                TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(
      this IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> repo,
      IDataModelQueryManyCommand<TDataModel, TDbConnection, 
        TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> cmd,
      TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6,
      TParam7 param7, TParam8 param8, TParam9 param9)
      where TDbConnection : DbConnection
    {
      Contract.Requires<ArgumentNullException>(repo != null);
      Contract.Requires<ArgumentNullException>(cmd != null);
      Contract.Ensures(Contract.Result<IDataModelQueryResult<TDataModel>>() != null);
      return repo.ExecuteMany(cmd, DbContext.Current, QueryBehavior.Default, param, param1, param2, param3, param4,
        param5, param6, param7, param8, param9);
    }

  }
}