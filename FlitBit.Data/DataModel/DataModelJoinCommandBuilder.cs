using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;
using FlitBit.Data.SqlServer;

namespace FlitBit.Data.DataModel
{
  /// <summary>
  ///   Builds SQL commands over a data model.
  /// </summary>
  /// <typeparam name="TDataModel">data model's type</typeparam>
  /// <typeparam name="TIdentityKey"></typeparam>
  /// <typeparam name="TDbConnection">db connection type</typeparam>
  /// <typeparam name="TJoin"></typeparam>
  public abstract class DataModelJoinCommandBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin> 
    : DataModelCommandBuilder<TDataModel>,
    IDataModelJoinCommandBuilder<TDataModel, TDbConnection, TJoin>
  {
    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="binder"></param>
    /// <param name="queryKey">the query's key</param>
    /// <param name="sqlWriter">a writer</param>
    protected DataModelJoinCommandBuilder(IDataModelBinder<TDataModel, TIdentityKey> binder, string queryKey,
      DataModelSqlWriter<TDataModel> sqlWriter)
      : base(binder, queryKey, sqlWriter)
    {}

    /// <summary>
    ///   Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
    ///   SQL.
    /// </summary>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    public IDataModelQueryCommand<TDataModel, TDbConnection, TParam> Where<TParam>(
      Expression<Func<TDataModel, TJoin, TParam, bool>> predicate)
    {
      var lambda = (LambdaExpression)predicate;

      var sql = new DataModelSqlExpression<TDataModel>(Mapping, Binder, Writer.SelfRef);
      var parms = new List<ParameterExpression>(lambda.Parameters);
      sql.AddSelfParameter(parms[0]);
      sql.JoinParameter(parms[1], false);
      sql.AddValueParameter(parms[2]);

      sql.IngestExpresion(lambda.Body);
      var writer = new SqlWriter().Append(this.Writer.Select.Text);
      sql.Write(writer);
      
      throw new NotImplementedException();
      //return ConstructCommandOnConstraints<TParam>(
      //  PrepareTranslateExpression(cns, lambda.Body)
      //  );
    }

    /// <summary>
    ///   Builds a query command with the specified constraints.
    /// </summary>
    /// <param name="sql">the prepared sql expression</param>
    /// <returns></returns>
    protected abstract IDataModelQueryCommand<TDataModel, TDbConnection, TParam> ConstructCommandOnConstraints<TParam>(
      DataModelSqlExpression<TDataModel> sql);
  }
}