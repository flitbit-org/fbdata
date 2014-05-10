#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;

namespace FlitBit.Data.DataModel
{
  /// <summary>
  ///   Binds a model to an underlying database structure.
  /// </summary>
  public interface IDataModelBinder
  {
    /// <summary>
    ///   Gets the model's mapping (untyped).
    /// </summary>
    IMapping UntypedMapping { get; }

    /// <summary>
    ///   Indicates the binder's mapping stretegy.
    /// </summary>
    MappingStrategy Strategy { get; }

    /// <summary>
    ///   Adds the model's DDL to a string builder.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="members"></param>
    void BuildDdlBatch(StringBuilder batch, IList<Type> members);

    /// <summary>
    ///   Initializes the binder.
    /// </summary>
    void Initialize();
  }

  /// <summary>
  ///   Binds a model to an underlying database structure.
  /// </summary>
  /// <typeparam name="TModel">the model's type.</typeparam>
  /// <typeparam name="TIdentityKey">the model's identity type</typeparam>
  public interface IDataModelBinder<TModel> : IDataModelBinder
  {
    /// <summary>
    ///   Gets the model's mapping.
    /// </summary>
    IMapping<TModel> Mapping { get; }
  }

  /// <summary>
  ///   Binds a model to an underlying database structure.
  /// </summary>
  /// <typeparam name="TModel">the model's type.</typeparam>
  /// <typeparam name="TIdentityKey">the model's identity type</typeparam>
  public interface IDataModelBinder<TModel, TIdentityKey> : IDataModelBinder<TModel>
  {
    /// <summary>
    ///   Makes a repository for the data model.
    /// </summary>
    /// <returns></returns>
    IDataModelRepository<TModel, TIdentityKey> MakeRepository();
  }

  /// <summary>
  ///   Binds a model to an underlying database structure.
  /// </summary>
  /// <typeparam name="TModel">the model's type.</typeparam>
  /// <typeparam name="TIdentityKey">the model's identity type</typeparam>
  /// <typeparam name="TDbConnection">database connection type TDbConnection</typeparam>
  public interface IDataModelBinder<TModel, TIdentityKey, TDbConnection> : IDataModelBinder<TModel, TIdentityKey>
    where TDbConnection : DbConnection
  {
    IDataModelWriter<TModel> Writer { get; }

    /// <summary>
    ///   Gets a model command for selecting all models of the type TModel.
    /// </summary>
    /// <returns></returns>
    IDataModelQueryManyCommand<TModel, TDbConnection> GetAllCommand();

    /// <summary>
    ///   Gets a create command.
    /// </summary>
    /// <returns></returns>
    IDataModelQuerySingleCommand<TModel, TDbConnection, TModel> GetCreateCommand();

    /// <summary>
    ///   Gets a delete (by ID) command.
    /// </summary>
    /// <returns></returns>
    IDataModelNonQueryCommand<TModel, TDbConnection, TIdentityKey> GetDeleteCommand();

    /// <summary>
    ///   Gets a read (by ID) command.
    /// </summary>
    /// <returns></returns>
    IDataModelQuerySingleCommand<TModel, TDbConnection, TIdentityKey> GetReadCommand();

    /// <summary>
    ///   Gets an update command.
    /// </summary>
    /// <returns></returns>
    IDataModelQuerySingleCommand<TModel, TDbConnection, TModel> GetUpdateCommand();

    object ConstructQueryCommand(IDataModelRepository<TModel, TIdentityKey, TDbConnection> repo, string key,
      DataModelSqlExpression<TModel> sql, IDataModelWriter<TModel> writer);
  }
}