#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using FlitBit.Data.Expressions;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data.DataModel
{
  public class BasicDataModelQueryBuilder<TDataModel, TIdentityKey, TDbConnection>
    where TDbConnection : DbConnection
  {
    public BasicDataModelQueryBuilder(IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> repo,
      IDataModelWriter<TDataModel> writer)
      : this(repo, writer, Guid.NewGuid().ToString("N"))
    {}

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="repo">the data model's repository</param>
    /// <param name="writer"></param>
    /// <param name="key"></param>
    public BasicDataModelQueryBuilder(IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> repo,
      IDataModelWriter<TDataModel> writer, string key)
    {
      Contract.Requires<ArgumentNullException>(repo != null);
      Contract.Requires<ArgumentNullException>(writer != null);
      Contract.Requires<ArgumentNullException>(key != null);
      Contract.Requires<ArgumentException>(key.Length > 0);
      Key = key;
      Repository = repo;
      Writer = writer;
    }

    /// <summary>
    ///   The data model's repository.
    /// </summary>
    public IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> Repository { get; private set; }

    /// <summary>
    ///   The query's identity key.
    /// </summary>
    [IdentityKey]
    public string Key { get; private set; }

    /// <summary>
    ///   Gets the data model's writer.
    /// </summary>
    protected IDataModelWriter<TDataModel> Writer { get; private set; }

    /// <summary>
    /// Creates a query command from the expression tree specified.
    /// </summary>
    /// <param name="lambda"></param>
    /// <returns></returns>
    protected object ConstructQueryCommandFromExpression(LambdaExpression lambda, Action<OrderByBuilder<TDataModel>, TDataModel> orderByClause)
    {
      Contract.Requires<ArgumentNullException>(lambda != null);
      Contract.Ensures(Contract.Result<object>() != null);

      var parms = new List<ParameterExpression>(lambda.Parameters);

      var repo = Repository;
      DataModelSqlExpression<TDataModel> sql = (orderByClause != null)
                                                 ? new DataModelSqlExpression<TDataModel>(repo.Binder.Mapping, repo.Binder,
                                                     Writer.SelfRef,
                                                     exp => new OrderByBuilder<TDataModel>(exp),
                                                     orderByClause)
                                                 : new DataModelSqlExpression<TDataModel>(repo.Binder.Mapping,
                                                     repo.Binder, Writer.SelfRef);
      sql.AddSelfParameter(parms[0]);
      for (var i = 1; i < parms.Count; i++)
      {
        sql.AddValueParameter(parms[i]);
      }
      sql.IngestExpression(lambda.Body);
      return Repository.ConstructQueryCommand(Key, sql);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection> OrderBy(
      Action<OrderByBuilder<TDataModel>, TDataModel> orderByClause)
    {
      Contract.Requires<ArgumentNullException>(orderByClause != null);
      var self = Expression.Parameter(typeof(TDataModel), "self");

      var repo = Repository;
      var sql = new DataModelSqlExpression<TDataModel>(repo.Binder.Mapping, repo.Binder, Writer.SelfRef,
        exp => new OrderByBuilder<TDataModel>(exp),
        orderByClause);
      sql.AddSelfParameter(self);
      
      return (IDataModelQueryCommand<TDataModel, TDbConnection>)Repository.ConstructQueryCommand(Key, sql);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection> MakeCommand()
    {
      var self = Expression.Parameter(typeof(TDataModel), "self");

      var repo = Repository;
      var sql = new DataModelSqlExpression<TDataModel>(repo.Binder.Mapping, repo.Binder, Writer.SelfRef);
      sql.AddSelfParameter(self);

      return (IDataModelQueryCommand<TDataModel, TDbConnection>)Repository.ConstructQueryCommand(Key, sql);
    }
  }

  /// <summary>
  ///   Base class for building data model commands.
  /// </summary>
  /// <typeparam name="TDataModel"></typeparam>
  /// <typeparam name="TDbConnection"></typeparam>
  /// <typeparam name="TIdentityKey"></typeparam>
  public class DataModelQueryBuilder<TDataModel, TIdentityKey, TDbConnection>
    : BasicDataModelQueryBuilder<TDataModel, TIdentityKey, TDbConnection>
      , IDataModelQueryBuilder<TDataModel, TIdentityKey, TDbConnection>
    where TDbConnection : DbConnection
  {
    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="repo">the data model's repository</param>
    /// <param name="writer"></param>
    public DataModelQueryBuilder(IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> repo,
      IDataModelWriter<TDataModel> writer)
      : base(repo, writer)
    {
      Contract.Requires<ArgumentNullException>(repo != null);
      Contract.Requires<ArgumentNullException>(writer != null);
    }

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="repo">the data model's repository</param>
    /// <param name="writer"></param>
    /// <param name="key"></param>
    public DataModelQueryBuilder(IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> repo,
      IDataModelWriter<TDataModel> writer, string key)
      : base(repo, writer, key)
    {
      Contract.Requires<ArgumentNullException>(repo != null);
      Contract.Requires<ArgumentNullException>(writer != null);
    }

    public IDataModelJoinQueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin> Join<TJoin>()
    {
      var repo = Repository;
      var sql = new DataModelSqlExpression<TDataModel>(repo.Binder.Mapping, repo.Binder, Writer.SelfRef);
      sql.AddSelfParameter(Expression.Parameter(typeof(TDataModel), Writer.SelfName));
      sql.AddJoinParameter(Expression.Parameter(typeof(TJoin), "join"), true);

      return new DataModelJoinQueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin>(repo, Writer, Key, sql);
    }

    public IDataModelJoinQueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin> Join<TJoin>(
      Expression<Func<TDataModel, TJoin, bool>> predicate)
    {
      var lambda = (LambdaExpression)predicate;
      var parms = new List<ParameterExpression>(lambda.Parameters);

      var repo = Repository;
      var sql = new DataModelSqlExpression<TDataModel>(repo.Binder.Mapping, repo.Binder, Writer.SelfRef);
      sql.AddSelfParameter(parms[0]);
      sql.AddJoinParameter(parms[1], false);
      sql.IngestJoinExpression(parms[1], lambda.Body);

      return new DataModelJoinQueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin>(repo, Writer, Key, sql);
    }

    public IDataModelJoinQueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin, TParam> Join<TJoin, TParam>(
      Expression<Func<TDataModel, TJoin, TParam, bool>> predicate)
    {
      var lambda = (LambdaExpression)predicate;
      var parms = new List<ParameterExpression>(lambda.Parameters);

      var repo = Repository;
      var sql = new DataModelSqlExpression<TDataModel>(repo.Binder.Mapping, repo.Binder, Writer.SelfRef);
      sql.AddSelfParameter(parms[0]);
      sql.AddJoinParameter(parms[1], false);
      sql.AddValueParameter(parms[2]);
      sql.IngestJoinExpression(parms[1], lambda.Body);

      return new DataModelJoinQueryBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin, TParam>(repo, Writer, Key,
        sql);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam> Where<TParam>(
      Expression<Func<TDataModel, TParam, bool>> predicate)
    {
      return
        (IDataModelQueryCommand<TDataModel, TDbConnection, TParam>)
        ConstructQueryCommandFromExpression(predicate, null);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam> Where<TParam>(
      Expression<Func<TDataModel, TParam, bool>> predicate, Action<OrderByBuilder<TDataModel>, TDataModel> orderByClause)
    {
      return
        (IDataModelQueryCommand<TDataModel, TDbConnection, TParam>)
        ConstructQueryCommandFromExpression(predicate, orderByClause);
    }

    /// <summary>
    ///   Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
    ///   SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1> Where<TParam, TParam1>(
      Expression<Func<TDataModel, TParam, TParam1, bool>> predicate)
    {
      return
        (IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1>)
        ConstructQueryCommandFromExpression(predicate, null);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1> Where<TParam, TParam1>(
      Expression<Func<TDataModel, TParam, TParam1, bool>> predicate, Action<OrderByBuilder<TDataModel>, TDataModel> orderByClause)
    {
      return
        (IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1>)
        ConstructQueryCommandFromExpression(predicate, orderByClause);
    }

    /// <summary>
    ///   Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
    ///   SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> Where<TParam, TParam1, TParam2>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, bool>>predicate)
    {
      return
        (IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2>)
        ConstructQueryCommandFromExpression(predicate, null);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> Where<TParam, TParam1, TParam2>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, bool>> predicate, 
      Action<OrderByBuilder<TDataModel>, TDataModel> orderByClause)
    {
      return
        (IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2>)
        ConstructQueryCommandFromExpression(predicate, orderByClause);
    }


    /// <summary>
    ///   Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
    ///   SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    public IDataModelQueryCommand <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> Where<TParam, TParam1, TParam2, TParam3>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, bool>>predicate)
    {
      return
        (IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3>)
        ConstructQueryCommandFromExpression(predicate, null);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> Where<TParam, TParam1, TParam2, TParam3>(
  Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, bool>> predicate,
      Action<OrderByBuilder<TDataModel>, TDataModel> orderByClause)
    {
      return
        (IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3>)
        ConstructQueryCommandFromExpression(predicate, orderByClause);
    }

    /// <summary>
    ///   Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
    ///   SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> Where<TParam, TParam1, TParam2, TParam3, TParam4>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, bool>>predicate)
    {
      return
        (IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4>)
        ConstructQueryCommandFromExpression(predicate, null);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> Where<TParam, TParam1, TParam2, TParam3, TParam4>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, bool>> predicate,
      Action<OrderByBuilder<TDataModel>, TDataModel> orderByClause)
    {
      return
        (IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4>)
        ConstructQueryCommandFromExpression(predicate, orderByClause);
    }

    /// <summary>
    ///   Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
    ///   SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <typeparam name="TParam5"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, bool>>predicate)
    {
      return
        (IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>)
        ConstructQueryCommandFromExpression(predicate, null);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(
     Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, bool>> predicate,
      Action<OrderByBuilder<TDataModel>, TDataModel> orderByClause)
    {
      return
        (IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>)
        ConstructQueryCommandFromExpression(predicate, orderByClause);
    }
    /// <summary>
    ///   Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
    ///   SQL.
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
    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool>>predicate)
    {
      return
        (IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>
        )
        ConstructQueryCommandFromExpression(predicate, null);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool>> predicate,
      Action<OrderByBuilder<TDataModel>, TDataModel> orderByClause)
    {
      return
        (IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>
        )
        ConstructQueryCommandFromExpression(predicate, orderByClause);
    }

    /// <summary>
    ///   Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
    ///   SQL.
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool>> predicate)
    {
      return
        (
        IDataModelQueryCommand
          <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>)
        ConstructQueryCommandFromExpression(predicate, null);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool>> predicate,
      Action<OrderByBuilder<TDataModel>, TDataModel> orderByClause)
    {
      return
        (
        IDataModelQueryCommand
          <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>)
        ConstructQueryCommandFromExpression(predicate, orderByClause);
    }

    /// <summary>
    ///   Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
    ///   SQL.
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
    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, bool>> predicate)
    {
      return
        (
        IDataModelQueryCommand
          <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>)
        ConstructQueryCommandFromExpression(predicate, null);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
  Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, bool>> predicate,
      Action<OrderByBuilder<TDataModel>, TDataModel> orderByClause)
    {
      return
        (
        IDataModelQueryCommand
          <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>)
        ConstructQueryCommandFromExpression(predicate, orderByClause);
    }

    /// <summary>
    ///   Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
    ///   SQL.
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
    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, bool>> predicate)
    {
      return
        (
        IDataModelQueryCommand
          <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8,
          TParam9>)
        ConstructQueryCommandFromExpression(predicate, null);
    }

    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(
      Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, bool>> predicate,
      Action<OrderByBuilder<TDataModel>, TDataModel> orderByClause)
    {
      return
        (
        IDataModelQueryCommand
          <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8,
          TParam9>)
        ConstructQueryCommandFromExpression(predicate, orderByClause);
    }
  }
}