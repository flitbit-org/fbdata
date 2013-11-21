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
  public abstract class BaseSqlDataModelBinder<TDataModel, TIdentityKey, TModelImpl> : DataModelBinder<TDataModel, TIdentityKey, SqlConnection>
    where TModelImpl : class, TDataModel, IDataModel, new()
  {
    bool _initialized;
    
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Tuple<int, IDataModelQueryManyCommand<TDataModel, SqlConnection>> _selectAll;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Tuple<int, IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel>> _create;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Tuple<int, IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel>> _update;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Tuple<int, IDataModelQuerySingleCommand<TDataModel, SqlConnection, TIdentityKey>> _read;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Tuple<int, IDataModelNonQueryCommand<TDataModel, SqlConnection, TIdentityKey>> _delete;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Tuple<int, IDataModelRepository<TDataModel, TIdentityKey>> _repository;

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="mapping"></param>
    /// <param name="strategy"></param>
    protected BaseSqlDataModelBinder(IMapping<TDataModel> mapping, MappingStrategy strategy)
      : base(mapping, strategy)
    {
      Contract.Requires<ArgumentNullException>(mapping != null);

      this.Writer = new DataModelSqlWriter<TDataModel>(mapping.QuoteObjectName("self"), "  ");
      this.Offsets = Enumerable.Range(0, this.Writer.QuotedColumnNames.Length).ToArray();
    }

    protected int[] Offsets { get; private set; }

    protected DataModelSqlWriter<TDataModel> Writer { get; private set; } 
   
    public override void Initialize()
    {
      if (!this._initialized)
      {
        PerformInitialization();
        this._initialized = true;
      }
    }

    protected abstract void PerformInitialization();

    public override IDataModelQueryManyCommand<TDataModel, SqlConnection> GetAllCommand()
    {
      var mapping = this.Mapping;
      if (this._selectAll == null || this._selectAll.Item1 < mapping.Revision)
      {
        this._selectAll = Tuple.Create(mapping.Revision,
          (IDataModelQueryManyCommand<TDataModel, SqlConnection>)
          ConstructGetAllCommand());
      }
      return this._selectAll.Item2;
    }

    protected abstract IDataModelQueryManyCommand<TDataModel, SqlConnection> ConstructGetAllCommand();

    public override IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel> GetCreateCommand()
    {
      var mapping = this.Mapping;
      if (this._create == null || this._create.Item1 < mapping.Revision)
      {
        this._create = Tuple.Create(mapping.Revision,
          (IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel>)
          ConstructGetCreateCommand());
      }
      return this._create.Item2;
    }

    protected abstract IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel> ConstructGetCreateCommand();

    public override IDataModelNonQueryCommand<TDataModel, SqlConnection, TIdentityKey> GetDeleteCommand()
    {
      var mapping = this.Mapping;
      if (this._delete == null || this._delete.Item1 < mapping.Revision)
      {
        this._delete = Tuple.Create(mapping.Revision,
          (IDataModelNonQueryCommand<TDataModel, SqlConnection, TIdentityKey>)
          ConstructGetDeleteCommand());
      }
      return this._delete.Item2;
    }

    protected abstract IDataModelNonQueryCommand<TDataModel, SqlConnection, TIdentityKey> ConstructGetDeleteCommand();

    public override IDataModelQuerySingleCommand<TDataModel, SqlConnection, TIdentityKey> GetReadCommand()
    {
      var mapping = this.Mapping;
      if (this._read == null || this._read.Item1 < mapping.Revision)
      {
        this._read = Tuple.Create(mapping.Revision,
          (IDataModelQuerySingleCommand<TDataModel, SqlConnection, TIdentityKey>)
          ConstructReadCommand());
      }
      return this._read.Item2;
    }

    protected abstract IDataModelQuerySingleCommand<TDataModel, SqlConnection, TIdentityKey> ConstructReadCommand();

    public override IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel> GetUpdateCommand()
    {
      var mapping = this.Mapping;
      if (this._update == null || this._update.Item1 < mapping.Revision)
      {
        this._update = Tuple.Create(mapping.Revision,
          (IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel>)
          ConstructUpdateCommand());
      }
      return this._update.Item2;
    }

    protected abstract IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel> ConstructUpdateCommand();

    public override IDataModelRepository<TDataModel, TIdentityKey> MakeRepository()
    {
      var mapping = this.Mapping;
      if (this._repository == null || this._repository.Item1 < mapping.Revision)
      {
        this._repository = Tuple.Create(mapping.Revision,
          (IDataModelRepository<TDataModel, TIdentityKey>)
          ConstructRepository());
      }
      return this._repository.Item2;
    }

    protected abstract IDataModelRepository<TDataModel, TIdentityKey> ConstructRepository();

    public override IDataModelCommandBuilder<TDataModel, SqlConnection, TCriteria> MakeQueryCommand<TCriteria>(string queryKey, TCriteria input)
    {
      return new SqlDataModelCommandBuilder<TDataModel, TModelImpl, TCriteria>(this, queryKey, Writer);
    }
    public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam> MakeQueryCommand<TParam>(string queryKey)
    {
      return new SqlDataModelCommandBuilder<TDataModel, TModelImpl, TParam>(this, queryKey, Writer);
    }
    public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1> MakeQueryCommand
      <TParam, TParam1>(string queryKey)
    {
      return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1>(this, queryKey, Writer);
    }
    public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2> MakeQueryCommand
      <TParam, TParam1, TParam2>(string queryKey)
    {
      return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2>(this, queryKey, Writer);
    }
    public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3> MakeQueryCommand
      <TParam, TParam1, TParam2, TParam3>(string queryKey)
    {
      return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2, TParam3>(this, queryKey, Writer);
    }
    public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4> MakeQueryCommand
      <TParam, TParam1, TParam2, TParam3, TParam4>(string queryKey)
    {
      return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4>(this, queryKey, Writer);
    }
    public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> MakeQueryCommand
      <TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(string queryKey)
    {
      return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(this, queryKey, Writer);
    }
    public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> MakeQueryCommand
      <TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(string queryKey)
    {
      return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(this, queryKey, Writer);
    }
    public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> MakeQueryCommand
      <TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(string queryKey)
    {
      return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(this, queryKey, Writer);
    }
    public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> MakeQueryCommand
      <TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(string queryKey)
    {
      return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(this, queryKey, Writer);
    }
    public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> MakeQueryCommand
      <TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(string queryKey)
    {
      return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(this, queryKey, Writer);
    }

    public override IDataModelJoinCommandBuilder<TDataModel, SqlConnection, TJoin> MakeJoinCommand<TJoin>(string queryKey)
    {
      return new SqlDataModelJoinCommandBuilder<TDataModel, TIdentityKey, TModelImpl, TJoin>(this, queryKey, Writer);
    }
  }
}