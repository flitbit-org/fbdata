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
		static readonly string SelectPageFormat = @"DECLARE @first_id int
DECLARE @totrows int

SELECT @totrows = SUM (row_count)
FROM sys.dm_db_partition_stats
WHERE object_id=OBJECT_ID('{0}')   
AND (index_id=0 or index_id=1);

SET ROWCOUNT @startRow
SELECT @first_id = {2} 
	FROM {0}
	ORDER BY {2}

SET ROWCOUNT @limit
SELECT {1},
       @totrows AS TotRows
FROM {0}
WHERE {2} >= @first_id
ORDER BY {2}
SET ROWCOUNT 0
";
		static readonly string GeneratedTimestamp = "DECLARE @generated_timestamp DATETIME2 = GETUTCDATE()";

		static readonly string CreateFormat = @"
DECLARE @identity INT

INSERT INTO {0} (
  $(columns)
)
VALUES (
  $(values)
)

SET @identity = SCOPE_IDENTITY()
SELECT {1}
FROM {0}
WHERE {2} = @identity
";
		static readonly string UpdateFormat = @"
UPDATE {0}
	SET $(columns)
WHERE {2} = @identity

SELECT {1}
FROM {0}
WHERE {2} = @identity";

		bool _initialized;
		readonly string[] _columns;
		readonly int[] _offsets;
		readonly string _selectSql;

		readonly IDataModelQueryManyCommand<TModel, SqlConnection> _selectAll;
		readonly IDataModelQuerySingleCommand<TModel, SqlConnection, TModel> _create;
		readonly IDataModelQuerySingleCommand<TModel, SqlConnection, TModel> _update;
		readonly IDataModelQuerySingleCommand<TModel, SqlConnection, TIdentityKey> _byId;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="mapping"></param>
		public OneClassOneTableBinder(Mapping<TModel> mapping)
			: base(mapping, MappingStrategy.OneClassOneTable)
		{
			Contract.Requires<ArgumentNullException>(mapping != null);
			Contract.Requires<ArgumentException>(mapping.Strategy == MappingStrategy.OneClassOneTable);

			_columns = mapping.Columns.Select(c => mapping.QuoteObjectNameForSQL(c.TargetName)).ToArray();
			_offsets = Enumerable.Range(0, _columns.Length).ToArray();

			var usesTimestamp = mapping.Columns.Count(c => c.IsTimestampOnInsert || c.IsTimestampOnUpdate);
			if (mapping.Identity.Columns.Count() == 1)
			{
				var idCol = mapping.QuoteObjectNameForSQL(mapping.Identity.Columns[0].TargetName);
				var columnList = String.Join(String.Concat(",", Environment.NewLine, "       "), _columns);

				var writer = new SqlWriter();
				var i = 0;
				var tableRef = mapping.QuoteObjectNameForSQL("self");
				writer.Append("SELECT ");
				foreach (var c in _columns)
				{
					if (i++ > 0)
					{
						writer.NewLine().Append(tableRef).Append('.').Append(c).Append(',');
					}
					else
					{
						writer.Append(tableRef).Append('.').Append(c).Append(',');
						writer.Indent();
					}
				}
				writer.Outdent().NewLine()
					.Append("FROM ").Append(mapping.DbObjectReference)
					.Append(" AS ").Append(tableRef);

				_selectSql = writer.ToString();
				var selectPage = String.Format(SelectPageFormat, mapping.DbObjectReference, columnList, idCol);

				_selectAll = new DataModelQueryManyCommand<TModel, TModelImpl>(_selectSql, selectPage, _offsets);
				var create = (mapping.Columns.Any(c => c.IsTimestampOnInsert || c.IsTimestampOnUpdate))
 					? String.Format(GeneratedTimestamp, CreateFormat, mapping.DbObjectReference, columnList, idCol).Replace("$(columns)", "{0}").Replace("$(values)", "{1}")
					: String.Format(CreateFormat, mapping.DbObjectReference, columnList, idCol).Replace("$(columns)", "{0}").Replace("$(values)", "{1}");
				_create =
					(IDataModelQuerySingleCommand<TModel, SqlConnection, TModel>)
						Activator.CreateInstance(OneClassOneTableEmitter.CreateCommand<TModel, TModelImpl>(mapping), create, _offsets);
				var update = (mapping.Columns.Any(c => c.IsTimestampOnUpdate))
					? String.Format(GeneratedTimestamp, UpdateFormat, mapping.DbObjectReference, columnList, idCol).Replace("$(columns)", "{0}")
					: String.Format(UpdateFormat, mapping.DbObjectReference, columnList, idCol).Replace("$(columns)", "{0}");
				
				_update =
					(IDataModelQuerySingleCommand<TModel, SqlConnection, TModel>)
						Activator.CreateInstance(OneClassOneTableEmitter.UpdateCommand<TModel, TModelImpl>(mapping), update, _offsets);

				_byId =
					(IDataModelQuerySingleCommand<TModel, SqlConnection, TIdentityKey>)
						Activator.CreateInstance(OneClassOneTableEmitter.ReadByIdCommand<TModel, TModelImpl, TIdentityKey>(mapping), update, _offsets);

			}
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
			return _selectAll;
		}

		public override IDataModelQuerySingleCommand<TModel, SqlConnection, TModel> GetCreateCommand()
		{
			return _create;
		}

		public override IDataModelNonQueryCommand<TModel, SqlConnection, TIdentityKey> GetDeleteCommand()
		{
			throw new NotImplementedException();
		}

		public override IDataModelQuerySingleCommand<TModel, SqlConnection, TIdentityKey> GetReadCommand()
		{
			return _byId;
		}

		public override IDataModelQuerySingleCommand<TModel, SqlConnection, TModel> GetUpdateCommand()
		{
			return _update;
		}
		
		public override IDataModelCommandBuilder<TModel, SqlConnection, TCriteria> MakeQueryCommand<TCriteria>(TCriteria input)
		{
			return new SqlDataModelCommandBuilder<TModel, TModelImpl, TCriteria>(this.Mapping, this._selectSql);
		}
	}
}