using System.Diagnostics;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace FlitBit.Data.SqlServer
{
	/// <summary>
	///   Binds a model using the dynamic hybrid inheritance tree strategy.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TIdentityKey"></typeparam>
	/// <typeparam name="TModelImpl"></typeparam>
	public class OneClassOneTableBinder<TDataModel, TIdentityKey, TModelImpl> : DataModelBinder<TDataModel, TIdentityKey, SqlConnection>
		where TModelImpl : class, TDataModel, IDataModel, new()
	{
		bool _initialized;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly DataModelSqlWriter<TDataModel> _sqlWriter;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly int[] _offsets;

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
		public OneClassOneTableBinder(IMapping<TDataModel> mapping)
			: base(mapping, MappingStrategy.OneClassOneTable)
		{
			Contract.Requires<ArgumentNullException>(mapping != null);
			Contract.Requires<ArgumentException>(mapping.Strategy == MappingStrategy.OneClassOneTable);

			_sqlWriter = new DataModelSqlWriter<TDataModel>(mapping.QuoteObjectName("self"), "  ");
			_offsets = Enumerable.Range(0, _sqlWriter.QuotedColumnNames.Length).ToArray();
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
					var dep in mapping.Dependencies.Where(d => d.Kind == DependencyKind.Base || d.Kind.HasFlag(DependencyKind.Direct) && d.Target != mapping))
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

		public override void Initialize()
		{
			if (!_initialized)
			{
				_initialized = true;
			}
		}

		public override IDataModelQueryManyCommand<TDataModel, SqlConnection> GetAllCommand()
		{
			var mapping = Mapping;
			if (_selectAll == null || _selectAll.Item1 < mapping.Revision)
			{
				_selectAll = Tuple.Create(mapping.Revision,
					(IDataModelQueryManyCommand<TDataModel, SqlConnection>)
					new DataModelQueryManyCommand<TDataModel, TModelImpl>(_sqlWriter.Select, _sqlWriter.SelectInPrimaryKeyOrderWithPaging, _offsets));
			}
			return _selectAll.Item2;
		}

		public override IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel> GetCreateCommand()
		{
			var mapping = Mapping;
			if (_create == null || _create.Item1 < mapping.Revision)
			{
				var createStatement = _sqlWriter.DynamicInsertStatement;
				_create = Tuple.Create(mapping.Revision,
					(IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel>)
						Activator.CreateInstance(OneClassOneTableEmitter.CreateCommand<TDataModel, TModelImpl>(mapping, createStatement), createStatement, _offsets));
			}
			return _create.Item2;
		}

		public override IDataModelNonQueryCommand<TDataModel, SqlConnection, TIdentityKey> GetDeleteCommand()
		{
			var mapping = Mapping;
			if (_delete == null || _delete.Item1 < mapping.Revision)
			{
				var deleteStatement = _sqlWriter.DeleteByPrimaryKey;

				_delete = Tuple.Create(mapping.Revision,
					(IDataModelNonQueryCommand<TDataModel, SqlConnection, TIdentityKey>)
						Activator.CreateInstance(OneClassOneTableEmitter.DeleteCommand<TDataModel, TModelImpl, TIdentityKey>(mapping, deleteStatement), deleteStatement, _offsets));
			}
			return _delete.Item2;
		}

		public override IDataModelQuerySingleCommand<TDataModel, SqlConnection, TIdentityKey> GetReadCommand()
		{
			var mapping = Mapping;
			if (_read == null || _read.Item1 < mapping.Revision)
			{
				var readStatement = _sqlWriter.SelectByPrimaryKey;
				_read = Tuple.Create(mapping.Revision,
					(IDataModelQuerySingleCommand<TDataModel, SqlConnection, TIdentityKey>)
						Activator.CreateInstance(OneClassOneTableEmitter.ReadByIdCommand<TDataModel, TModelImpl, TIdentityKey>(mapping, readStatement), readStatement, _offsets));
			}
			return _read.Item2;
		}

		public override IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel> GetUpdateCommand()
		{
			var mapping = Mapping;
			if (_update == null || _update.Item1 < mapping.Revision)
			{
				var updateStatement = _sqlWriter.DynamicUpdateStatement;
				_update = Tuple.Create(mapping.Revision,
					(IDataModelQuerySingleCommand<TDataModel, SqlConnection, TDataModel>)
						Activator.CreateInstance(OneClassOneTableEmitter.UpdateCommand<TDataModel, TModelImpl>(mapping, updateStatement), updateStatement, _offsets));
			}
			return _update.Item2;
		}

		public override IDataModelRepository<TDataModel, TIdentityKey> MakeRepository()
		{
			var mapping = Mapping;
			if (_repository == null || _repository.Item1 < mapping.Revision)
			{
				_repository = Tuple.Create(mapping.Revision,
					(IDataModelRepository<TDataModel, TIdentityKey>)
						Activator.CreateInstance(OneClassOneTableEmitter.MakeRepositoryType<TDataModel, TModelImpl, TIdentityKey>(mapping), (IMapping<TDataModel>)mapping));
			}
			return _repository.Item2;
		}
		
		public override IDataModelQueryCommandBuilder<TDataModel, SqlConnection, TCriteria> MakeQueryCommand<TCriteria>(string queryKey, TCriteria input)
		{
			return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TCriteria>(queryKey, _sqlWriter);
		}
		public override IDataModelQueryCommandBuilder<TDataModel, SqlConnection, TParam> MakeQueryCommand<TParam>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1> MakeQueryCommand
			<TParam, TParam1>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2> MakeQueryCommand
			<TParam, TParam1, TParam2>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3> MakeQueryCommand
			<TParam, TParam1, TParam2, TParam3>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2, TParam3>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4> MakeQueryCommand
			<TParam, TParam1, TParam2, TParam3, TParam4>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> MakeQueryCommand
			<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> MakeQueryCommand
			<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> MakeQueryCommand
			<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> MakeQueryCommand
			<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TDataModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> MakeQueryCommand
			<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TDataModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(queryKey, _sqlWriter);
		}
	}
}