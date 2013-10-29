using System;
using System.Collections.Generic;
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
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TIdentityKey"></typeparam>
	/// <typeparam name="TModelImpl"></typeparam>
	public class OneClassOneTableBinder<TModel, TIdentityKey, TModelImpl> : DataModelBinder<TModel, TIdentityKey, SqlConnection>
		where TModelImpl : class, TModel, IDataModel, new()
	{
		bool _initialized;
		private readonly DataModelSqlWriter<TModel> _sqlWriter; 
		readonly int[] _offsets;

		Tuple<int, IDataModelQueryManyCommand<TModel, SqlConnection>> _selectAll;
		Tuple<int, IDataModelQuerySingleCommand<TModel, SqlConnection, TModel>> _create;
		Tuple<int, IDataModelQuerySingleCommand<TModel, SqlConnection, TModel>> _update;
		Tuple<int, IDataModelQuerySingleCommand<TModel, SqlConnection, TIdentityKey>> _read;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="mapping"></param>
		public OneClassOneTableBinder(Mapping<TModel> mapping)
			: base(mapping, MappingStrategy.OneClassOneTable)
		{
			Contract.Requires<ArgumentNullException>(mapping != null);
			Contract.Requires<ArgumentException>(mapping.Strategy == MappingStrategy.OneClassOneTable);

			_sqlWriter = new DataModelSqlWriter<TModel>(mapping.QuoteObjectName("self"), "  ");
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
				var columnsWithTableConstraints = new List<Tuple<ColumnMapping<TModel>, object>>();
				var i = -1;
				foreach (ColumnMapping<TModel> col in mapping.Columns)
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

		public override IDataModelQueryManyCommand<TModel, SqlConnection> GetAllCommand()
		{
			var mapping = Mapping;
			if (_selectAll == null || _selectAll.Item1 < mapping.Revision)
			{
				_selectAll = Tuple.Create(mapping.Revision,
					(IDataModelQueryManyCommand<TModel, SqlConnection>)
					new DataModelQueryManyCommand<TModel, TModelImpl>(_sqlWriter.Select, _sqlWriter.SelectInPrimaryKeyOrderWithPaging, _offsets));
			}
			return _selectAll.Item2;
		}

		public override IDataModelQuerySingleCommand<TModel, SqlConnection, TModel> GetCreateCommand()
		{
			var mapping = Mapping;
			if (_create == null || _create.Item1 < mapping.Revision)
			{
				_create = Tuple.Create(mapping.Revision,
					(IDataModelQuerySingleCommand<TModel, SqlConnection, TModel>)
						Activator.CreateInstance(OneClassOneTableEmitter.CreateCommand<TModel, TModelImpl>(mapping), _sqlWriter.DynamicInsertStatement, _offsets));
			}
			return _create.Item2;
		}

		public override IDataModelNonQueryCommand<TModel, SqlConnection, TIdentityKey> GetDeleteCommand()
		{
			throw new NotImplementedException();
		}

		public override IDataModelQuerySingleCommand<TModel, SqlConnection, TIdentityKey> GetReadCommand()
		{
			var mapping = Mapping;
			if (_update == null || _update.Item1 < mapping.Revision)
			{
				_read = Tuple.Create(mapping.Revision,
					(IDataModelQuerySingleCommand<TModel, SqlConnection, TIdentityKey>)
						Activator.CreateInstance(OneClassOneTableEmitter.ReadByIdCommand<TModel, TModelImpl, TIdentityKey>(mapping), _sqlWriter.SelectByPrimaryKey, _offsets));
			}
			return _read.Item2;
		}

		public override IDataModelQuerySingleCommand<TModel, SqlConnection, TModel> GetUpdateCommand()
		{
			var mapping = Mapping;
			if (_update == null || _update.Item1 < mapping.Revision)
			{
				_update = Tuple.Create(mapping.Revision,
					(IDataModelQuerySingleCommand<TModel, SqlConnection, TModel>)
						Activator.CreateInstance(OneClassOneTableEmitter.UpdateCommand<TModel, TModelImpl>(mapping), _sqlWriter.DynamicUpdateStatement, _offsets));
			}
			return _update.Item2;
		}
		
		public override IDataModelCommandBuilder<TModel, SqlConnection, TCriteria> MakeQueryCommand<TCriteria>(TCriteria input)
		{
			return new SqlDataModelCommandBuilder<TModel, TModelImpl, TCriteria>(_sqlWriter);
		}

		public override IDataModelCommandBuilder<TModel, SqlConnection, TParam> MakeQueryCommand<TParam>()
		{
			return new SqlDataModelCommandBuilder<TModel, TModelImpl, TParam>(_sqlWriter);
		}

	}
}