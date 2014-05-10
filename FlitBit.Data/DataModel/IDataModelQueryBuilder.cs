#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Data.Common;
using System.Linq.Expressions;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data.DataModel
{
  /// <summary>
  /// Base data model query builder.
  /// </summary>
  /// <typeparam name="TDataModel"></typeparam>
  /// <typeparam name="TDbConnection"></typeparam>
  /// <typeparam name="TIdentityKey"></typeparam>
  public interface IDataModelQueryBuilder<TDataModel, TIdentityKey, TDbConnection>
    where TDbConnection: DbConnection
  {
    /// <summary>
    /// The data model's repository.
    /// </summary>
    IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> Repository { get; }

    /// <summary>
    /// Joins the data model to the specified type inferring the associated properties.
    /// </summary>
    /// <typeparam name="TJoin"></typeparam>
    /// <returns></returns>
    IDataModelJoinQueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin> Join<TJoin>();

    /// <summary>
    /// Joins the data model to the specified type according to the specified join predicate.
    /// </summary>
    /// <typeparam name="TJoin"></typeparam>
    /// <param name="predicate"></param>
    /// <returns></returns>
    IDataModelJoinQueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin> Join<TJoin>(
      Expression<Func<TDataModel, TJoin, bool>> predicate);

    /// <summary>
    /// Joins the data model to the specified type according to the specified join predicate.
    /// </summary>
    /// <typeparam name="TJoin"></typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <param name="predicate"></param>
    /// <returns></returns>
    IDataModelJoinQueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin, TParam> Join<TJoin, TParam>(
      Expression<Func<TDataModel, TJoin, TParam, bool>> predicate);

    /// <summary>
    /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    IDataModelQueryCommand<TDataModel, TDbConnection, TParam> Where<TParam>(
      Expression<Func<TDataModel, TParam, bool>> predicate);

    /// <summary>
    /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1> Where<TParam, TParam1>(
      Expression<Func<TDataModel, TParam, TParam1, bool>> predicate);

    /// <summary>
    /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> Where<TParam, TParam1, TParam2>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, bool>> predicate);

    /// <summary>
    /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> Where<TParam, TParam1, TParam2, TParam3>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, bool>> predicate);

    /// <summary>
    /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> Where<TParam, TParam1, TParam2, TParam3, TParam4>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, bool>> predicate);

    /// <summary>
    /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <typeparam name="TParam5"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, bool>> predicate);

    /// <summary>
    /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <typeparam name="TParam5"></typeparam>
    /// <typeparam name="TParam6"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool>> predicate);

    /// <summary>
    /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <typeparam name="TParam5"></typeparam>
    /// <typeparam name="TParam6"></typeparam>
    /// <typeparam name="TParam7"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool>> predicate);

    /// <summary>
    /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <typeparam name="TParam5"></typeparam>
    /// <typeparam name="TParam6"></typeparam>
    /// <typeparam name="TParam7"></typeparam>
    /// <typeparam name="TParam8"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, bool>> predicate);

    /// <summary>
    /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <typeparam name="TParam5"></typeparam>
    /// <typeparam name="TParam6"></typeparam>
    /// <typeparam name="TParam7"></typeparam>
    /// <typeparam name="TParam8"></typeparam>
    /// <typeparam name="TParam9"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, bool>> predicate);

    /// <summary>
    /// The query's identity key.
    /// </summary>
    [IdentityKey]
    string Key { get; }
  }
}