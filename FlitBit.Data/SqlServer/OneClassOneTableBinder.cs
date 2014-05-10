#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;

namespace FlitBit.Data.SqlServer
{
  /// <summary>
  ///   Binds a model using the dynamic hybrid inheritance tree strategy.
  /// </summary>
  /// <typeparam name="TDataModel"></typeparam>
  /// <typeparam name="TIdentityKey"></typeparam>
  /// <typeparam name="TModelImpl"></typeparam>
  public class OneClassOneTableBinder<TDataModel, TIdentityKey, TModelImpl> :
    BaseSqlDataModelBinder<TDataModel, TIdentityKey, TModelImpl>
    where TModelImpl : class, TDataModel, IDataModel, new()
  {

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="mapping"></param>
    public OneClassOneTableBinder(IMapping<TDataModel> mapping)
      : base(mapping, MappingStrategy.OneClassOneTable)
    {
      Contract.Requires<ArgumentNullException>(mapping != null);
      Contract.Requires<ArgumentException>(mapping.Strategy == MappingStrategy.OneClassOneTable);
    }



    public override void BuildDdlBatch(StringBuilder batch, IList<Type> members)
    {
      var mapping = Mapping;
      if (!members.Contains(mapping.RuntimeType))
      {
        members.Add(mapping.RuntimeType);
        if (string.IsNullOrEmpty(mapping.ConnectionName))
        {
          throw new MappingException("ConnectionName must be set before creating SQL commands for a data model.");
        }

        foreach (
          var dep in
            mapping.Dependencies.Where(
              d => d.Kind == DependencyKind.Base || d.Kind.HasFlag(DependencyKind.Direct) && d.Target != mapping))
        {
          var dmap = Mappings.AccessMappingFor(dep.Target.RuntimeType);
          var binder = dmap.GetBinder();
          binder.BuildDdlBatch(batch, members);
          batch.Append(Environment.NewLine)
               .Append("GO")
               .Append(Environment.NewLine);
        }

        if (!String.IsNullOrWhiteSpace(mapping.TargetSchema))
        {
          var providerHelper = mapping.GetDbProviderHelper();
          providerHelper.EmitCreateSchema(batch, mapping.TargetSchema);
        }
        batch.Append("CREATE TABLE ")
             .Append(mapping.DbObjectReference)
             .Append(Environment.NewLine)
             .Append('(');

        // Write each field's definition...
        var columnsWithTableConstraints = new List<Tuple<ColumnMapping<TDataModel>, object>>();
        var i = -1;
        foreach (ColumnMapping<TDataModel> col in mapping.Columns)
        {
          var handback = col.Emitter.EmitColumnDDL(batch, ++i, mapping, col);
          if (handback != null)
          {
            columnsWithTableConstraints.Add(Tuple.Create(col, handback));
          }
        }
        // Perform callback for each field with a table constraint...
        foreach (var it in columnsWithTableConstraints)
        {
          it.Item1.Emitter.EmitTableConstraintDDL(batch, mapping, it.Item1, it.Item2);
        }

        //if (!mapping.Behaviors.HasFlag(EntityBehaviors.DefinedColumnsOnly))
        //{
        //  sql.Append(',').Append(Environment.NewLine).Append(Environment.NewLine).Append("\t-- ETL Columns");
        //  sql.Append(Environment.NewLine).Append("\t[ETLHash] VARCHAR(32) NULL,");
        //  sql.Append(Environment.NewLine).Append("\t[ETLNaturalKey] VARCHAR(256) NULL,");
        //  sql.Append(Environment.NewLine).Append("\t[ETLCategory] VARCHAR(64) NULL");
        //}
        AddTableConstraintsForIndexes(mapping, batch);
        batch.Append(Environment.NewLine)
             .Append(')');
        AddIndexesForTable(mapping, batch);
      }
    }

    public override object ConstructQueryCommand(IDataModelRepository<TDataModel, TIdentityKey, SqlConnection> repo, string key, DataModelSqlExpression<TDataModel> sql, IDataModelWriter<TDataModel> writer)
    {
      var all = LegacyWriter.WriteSelect(sql);
      var paging = LegacyWriter.WriteSelectWithPaging(sql, null);

      var cmd = OneClassOneTableEmitter.MakeQueryCommand<TDataModel, TModelImpl>(Mapping, key, sql);
      return Activator.CreateInstance(cmd, all, paging, LegacyWriter.ColumnOffsets);
    }

    protected override IDataModelQueryManyCommand<TDataModel, SqlConnection> ConstructGetAllCommand()
    {
      return
        new SqlDataModelQueryManyCommand<TDataModel, TModelImpl>(
          LegacyWriter.Select,
          LegacyWriter.SelectInPrimaryKeyOrderWithPaging,
          Offsets);
    }

    protected override IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel> ConstructGetCreateCommand()
    {
      var createStatement = LegacyWriter.DynamicInsertStatement;

      return (IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel>)
             Activator.CreateInstance(
               OneClassOneTableEmitter.CreateCommand<TDataModel, TModelImpl>(Mapping, createStatement),
               createStatement,
               Offsets
               );
    }

    protected override IDataModelNonQueryCommand<TDataModel, SqlConnection, TIdentityKey> ConstructGetDeleteCommand()
    {
      var deleteStatement = LegacyWriter.DeleteByPrimaryKey;
      return
        (IDataModelNonQueryCommand<TDataModel, SqlConnection, TIdentityKey>)
        Activator.CreateInstance(
          OneClassOneTableEmitter.DeleteCommand<TDataModel, TModelImpl, TIdentityKey>(Mapping, deleteStatement),
          deleteStatement,
          Offsets
          );
    }

    protected override IDataModelQuerySingleCommand<TDataModel, SqlConnection, TIdentityKey> ConstructReadCommand()
    {
      var readStatement = LegacyWriter.SelectByPrimaryKey;
      return
        (IDataModelQuerySingleCommand<TDataModel, SqlConnection, TIdentityKey>)
        Activator.CreateInstance(
          OneClassOneTableEmitter.ReadByIdCommand<TDataModel, TModelImpl, TIdentityKey>(Mapping, readStatement),
          readStatement,
          Offsets
          );
    }

    protected override IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel> ConstructUpdateCommand()
    {
      var updateStatement = LegacyWriter.DynamicUpdateStatement;
      return
        (IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel>)
        Activator.CreateInstance(
          OneClassOneTableEmitter.UpdateCommand<TDataModel, TModelImpl>(Mapping, updateStatement),
          updateStatement,
          Offsets
          );
    }

    protected override IDataModelRepository<TDataModel, TIdentityKey> ConstructRepository()
    {
      return
        (IDataModelRepository<TDataModel, TIdentityKey>)
        Activator.CreateInstance(
          OneClassOneTableEmitter.MakeRepositoryType<TDataModel, TModelImpl, TIdentityKey>(Mapping),
          Mapping
          );
    }

    protected override void PerformInitialization() { }
  }
}