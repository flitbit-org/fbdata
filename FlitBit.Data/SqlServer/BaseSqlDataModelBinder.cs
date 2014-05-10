#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;

namespace FlitBit.Data.SqlServer
{
  /// <summary>
  ///   Abstract data model binder using a SqlConnection.
  /// </summary>
  /// <typeparam name="TDataModel"></typeparam>
  /// <typeparam name="TIdentityKey"></typeparam>
  /// <typeparam name="TModelImpl"></typeparam>
  public abstract class BaseSqlDataModelBinder<TDataModel, TIdentityKey, TModelImpl> :
    DataModelBinder<TDataModel, TIdentityKey, SqlConnection>
    where TModelImpl : class, TDataModel, IDataModel, new()
  {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private
      Tuple<int, IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel>> _create;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private
      Tuple<int, IDataModelNonQueryCommand<TDataModel, SqlConnection, TIdentityKey>> _delete;

    private bool _initialized;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private
      Tuple<int, IDataModelQuerySingleCommand<TDataModel, SqlConnection, TIdentityKey>> _read;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private Tuple<int, IDataModelRepository<TDataModel, TIdentityKey>>
      _repository;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private
      Tuple<int, IDataModelQueryManyCommand<TDataModel, SqlConnection>> _selectAll;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private
      Tuple<int, IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel>> _update;

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="mapping"></param>
    /// <param name="strategy"></param>
    protected BaseSqlDataModelBinder(IMapping<TDataModel> mapping, MappingStrategy strategy)
      : base(mapping, strategy)
    {
      Contract.Requires<ArgumentNullException>(mapping != null);

      LegacyWriter = new DataModelSqlWriter<TDataModel>(mapping.QuoteObjectName("self"), "  ");
      Offsets = Enumerable.Range(0, LegacyWriter.QuotedColumnNames.Length).ToArray();
    }

    protected int[] Offsets { get; private set; }

    public override IDataModelWriter<TDataModel> Writer
    {
      get { return LegacyWriter; }
    }

    protected DataModelSqlWriter<TDataModel> LegacyWriter { get; private set; }

    public override void Initialize()
    {
      if (!_initialized)
      {
        PerformInitialization();
        _initialized = true;
      }
    }

    protected abstract void PerformInitialization();

    public override IDataModelQueryManyCommand<TDataModel, SqlConnection> GetAllCommand()
    {
      IMapping<TDataModel> mapping = Mapping;
      if (_selectAll == null || _selectAll.Item1 < mapping.Revision)
      {
        _selectAll = Tuple.Create(mapping.Revision,
          ConstructGetAllCommand());
      }
      return _selectAll.Item2;
    }

    protected abstract IDataModelQueryManyCommand<TDataModel, SqlConnection> ConstructGetAllCommand();

    public override IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel> GetCreateCommand()
    {
      IMapping<TDataModel> mapping = Mapping;
      if (_create == null || _create.Item1 < mapping.Revision)
      {
        _create = Tuple.Create(mapping.Revision,
          ConstructGetCreateCommand());
      }
      return _create.Item2;
    }

    protected abstract IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel> ConstructGetCreateCommand();

    public override IDataModelNonQueryCommand<TDataModel, SqlConnection, TIdentityKey> GetDeleteCommand()
    {
      IMapping<TDataModel> mapping = Mapping;
      if (_delete == null || _delete.Item1 < mapping.Revision)
      {
        _delete = Tuple.Create(mapping.Revision,
          ConstructGetDeleteCommand());
      }
      return _delete.Item2;
    }

    protected abstract IDataModelNonQueryCommand<TDataModel, SqlConnection, TIdentityKey> ConstructGetDeleteCommand();

    public override IDataModelQuerySingleCommand<TDataModel, SqlConnection, TIdentityKey> GetReadCommand()
    {
      IMapping<TDataModel> mapping = Mapping;
      if (_read == null || _read.Item1 < mapping.Revision)
      {
        _read = Tuple.Create(mapping.Revision,
          ConstructReadCommand());
      }
      return _read.Item2;
    }

    protected abstract IDataModelQuerySingleCommand<TDataModel, SqlConnection, TIdentityKey> ConstructReadCommand();

    public override IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel> GetUpdateCommand()
    {
      IMapping<TDataModel> mapping = Mapping;
      if (_update == null || _update.Item1 < mapping.Revision)
      {
        _update = Tuple.Create(mapping.Revision,
          ConstructUpdateCommand());
      }
      return _update.Item2;
    }

    protected abstract IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel> ConstructUpdateCommand();

    public override IDataModelRepository<TDataModel, TIdentityKey> MakeRepository()
    {
      IMapping<TDataModel> mapping = Mapping;
      if (_repository == null || _repository.Item1 < mapping.Revision)
      {
        _repository = Tuple.Create(mapping.Revision,
          ConstructRepository());
      }
      return _repository.Item2;
    }

    protected abstract IDataModelRepository<TDataModel, TIdentityKey> ConstructRepository();

  }
}