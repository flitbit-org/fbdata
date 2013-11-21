using System;
using System.Data;
using System.Data.SqlClient;
using FlitBit.Data.DataModel;
using FlitBit.Data.Expressions;
using FlitBit.Data.SPI;

namespace FlitBit.Data.SqlServer
{
  public class SqlDataModelJoinCommandBuilder<TDataModel, TIdentityKey, TModelImpl, TJoin> :
    DataModelJoinCommandBuilder<TDataModel, TIdentityKey, SqlConnection, TJoin>
    where TModelImpl : class, IDataModel, TDataModel, new()
  {
    public SqlDataModelJoinCommandBuilder(IDataModelBinder<TDataModel, TIdentityKey> binder, string queryKey,
      DataModelSqlWriter<TDataModel> sqlWriter)
      : base(binder, queryKey, sqlWriter)
    {}

    /// <summary>
    ///   Builds a query command with the specified constraints.
    /// </summary>
    /// <param name="constraints"></param>
    /// <returns></returns>
    protected override IDataModelQueryCommand<TDataModel, SqlConnection, TParam> ConstructCommandOnConstraints
      <TParam>(Constraints constraints)
    {
      var cmd = OneClassOneTableEmitter.MakeQueryCommand<TDataModel, TModelImpl, TParam>(Mapping, QueryKey, constraints);
      var all = new DynamicSql(constraints.Writer.Text, CommandType.Text,
        CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
      return
        (IDataModelQueryCommand<TDataModel, SqlConnection, TParam>)
        Activator.CreateInstance(cmd, all, Writer.WriteSelectWithPaging(constraints, null), Writer.ColumnOffsets);
    }
  }
}