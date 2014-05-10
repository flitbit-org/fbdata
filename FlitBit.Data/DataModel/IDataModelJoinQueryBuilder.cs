#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Data.Common;
using System.Linq.Expressions;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data.DataModel
{
  public interface IDataModelJoinQueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin>
    where TDbConnection : DbConnection
  {
    IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> Repository { get; }

    /// <summary>
    /// Joins the data model to the specified type inferring the associated properties.
    /// </summary>
    /// <typeparam name="TJoin"></typeparam>
    /// <returns></returns>
    IDataModelJoin2QueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin, TJoin1> Join<TJoin1>();

    /// <summary>
    /// Joins the data model to the specified type according to the specified join predicate.
    /// </summary>
    /// <typeparam name="TJoin"></typeparam>
    /// <param name="predicate"></param>
    /// <returns></returns>
    IDataModelJoin2QueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin, TJoin1> Join<TJoin1>(
      Expression<Func<TDataModel, TJoin, TJoin1, bool>> predicate);

    /// <summary>
    /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    IDataModelQueryCommand<TDataModel, TDbConnection, TParam> Where<TParam>(
      Expression<Func<TDataModel, TJoin, TParam, bool>> predicate);

    /// <summary>
    /// The query's identity key.
    /// </summary>
    [IdentityKey]
    string Key { get; }
  }

  public interface IDataModelJoinQueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin, TParam>
    where TDbConnection : DbConnection
  {
    IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> Repository { get; }

    IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1> Where<TParam1>(
      Expression<Func<TDataModel, TJoin, TParam, bool>> predicate);

    IDataModelQueryCommand<TDataModel, TDbConnection, TParam> Construct();

    /// <summary>
    /// The query's identity key.
    /// </summary>
    [IdentityKey]
    string Key { get; }
  }

  public interface IDataModelJoin2QueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin, TJoin1>
    where TDbConnection : DbConnection
  {
    IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> Repository { get; }

    /// <summary>
    /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    IDataModelQueryCommand<TDataModel, TDbConnection, TParam> Where<TParam>(
      Expression<Func<TDataModel, TJoin, TJoin1, TParam, bool>> predicate);

    /// <summary>
    /// The query's identity key.
    /// </summary>
    [IdentityKey]
    string Key { get; }
  }

  public interface IDataModelJoin2QueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin, TJoin1, TParam>
    where TDbConnection : DbConnection
  {
    IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> Repository { get; }

    /// <summary>
    /// The query's identity key.
    /// </summary>
    [IdentityKey]
    string Key { get; }
  }
}