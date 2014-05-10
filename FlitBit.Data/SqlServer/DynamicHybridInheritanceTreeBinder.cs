#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TIdentityKey"></typeparam>
  /// <typeparam name="TModelImpl"></typeparam>
  public class DynamicHybridInheritanceTreeBinder<TModel, TIdentityKey, TModelImpl> :
    BaseSqlDataModelBinder<TModel, TIdentityKey, TModelImpl>
    where TModelImpl : class, IDataModel, TModel, new()
  {
    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="mapping"></param>
    /// <param name="strategy"></param>
    public DynamicHybridInheritanceTreeBinder(IMapping<TModel> mapping, MappingStrategy strategy)
      : base(mapping, strategy)
    {}

    public override void BuildDdlBatch(StringBuilder batch, IList<Type> members)
    {
      throw new NotImplementedException();
    }

    public override object ConstructQueryCommand(IDataModelRepository<TModel, TIdentityKey, SqlConnection> repo,
      string key, DataModelSqlExpression<TModel> sql, IDataModelWriter<TModel> writer)
    {
      throw new NotImplementedException();
    }

    protected override void PerformInitialization() { throw new NotImplementedException(); }

    protected override IDataModelQueryManyCommand<TModel, SqlConnection> ConstructGetAllCommand()
    {
      throw new NotImplementedException();
    }

    protected override IDataModelQuerySingleCommand<TModel, SqlConnection, TModel> ConstructGetCreateCommand()
    {
      throw new NotImplementedException();
    }

    protected override IDataModelNonQueryCommand<TModel, SqlConnection, TIdentityKey> ConstructGetDeleteCommand()
    {
      throw new NotImplementedException();
    }

    protected override IDataModelQuerySingleCommand<TModel, SqlConnection, TIdentityKey> ConstructReadCommand()
    {
      throw new NotImplementedException();
    }

    protected override IDataModelQuerySingleCommand<TModel, SqlConnection, TModel> ConstructUpdateCommand()
    {
      throw new NotImplementedException();
    }

    protected override IDataModelRepository<TModel, TIdentityKey> ConstructRepository()
    {
      throw new NotImplementedException();
    }
  }
}