using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using FlitBit.Core;
using FlitBit.Data.DataModel;
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
		DataModelBinder<TModel, TIdentityKey, SqlConnection>
		where TModelImpl : class, IDataModel, TModel, new()
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
		public DynamicHybridInheritanceTreeBinder(Mapping<TModel> mapping)
			: base(mapping, MappingStrategy.DynamicHybridInheritanceTree)
		{
			Contract.Requires<ArgumentNullException>(mapping != null);
			Contract.Requires<ArgumentException>(mapping.Strategy == MappingStrategy.DynamicHybridInheritanceTree);

			_sqlWriter = new DataModelSqlWriter<TModel>(mapping.QuoteObjectName("self"), "  ");
			_offsets = Enumerable.Range(0, _sqlWriter.QuotedColumnNames.Length).ToArray();
		}

		public override void BuildDdlBatch(StringBuilder batch, IList<Type> members)
		{
			var mapping = this.Mapping;

			if (!members.Contains(mapping.RuntimeType))
			{
				members.Add(mapping.RuntimeType);
				if (string.IsNullOrEmpty(mapping.ConnectionName))
				{
					throw new MappingException("ConnectionName must be set before creating SQL commands for a data model.");
				}
				
				foreach (
					var dep in mapping.Dependencies.Where(d => d.Kind == DependencyKind.Base || d.Kind.HasFlag(DependencyKind.Direct)))
				{
					var dmap = Mappings.AccessMappingFor(dep.Target.RuntimeType);
					var binder = dmap.GetBinder();
					binder.BuildDdlBatch(batch, members);
					batch.Append(Environment.NewLine)
							.Append("GO")
							.Append(Environment.NewLine);
				}

				batch.Append("CREATE TABLE ")
						.Append(mapping.DbObjectReference)
						.Append(Environment.NewLine)
						.Append('(');

				var i = -1;
				var idcol = mapping.Identity.Columns.Select(c => c.Column).SingleOrDefault();
				if (idcol == null)
				{
					throw new MappingException(
						String.Concat("Hierarchy mapping for `", mapping.RuntimeType.GetReadableSimpleName(),
													"` does not define an appropriate identity key. The current implementation only supports hierarchy mappings with a single synthetic identity column joining the hierarchy types.")
						);
				}

				// If not the base class, set up the hierarchy key with foreign key to base table's ID...
				IMapping baseMapping = null;
				var baseType = mapping.Dependencies.FirstOrDefault(d => d.Kind == DependencyKind.Base);
				while (baseType != null)
				{
					baseMapping = baseType.Target;
					baseType = baseMapping.Dependencies.FirstOrDefault(d => d.Kind == DependencyKind.Base);
				}
				if (baseMapping != null)
				{
					idcol.Emitter.EmitColumnDDLForHierarchy(batch, ++i, mapping, baseMapping, idcol);
				}

				// Write each field's definition...
				var columnsWithTableConstraints = new List<Tuple<ColumnMapping<TModel>, object>>();
				foreach (ColumnMapping<TModel> col in mapping.DeclaredColumns)
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
				if (mapping.Behaviors.HasFlag(EntityBehaviors.MapEnum))
				{
					if (!idcol.RuntimeType.IsEnum)
					{
						throw new MappingException(String.Concat("Model type '", typeof(TModel).Name,
																										"' declares behavior EntityBehaviors.MapEnum but the enum type cannot be determined. Specify an identity column of enum type."));
					}
					var namecol = mapping.Columns.FirstOrDefault(c => c.RuntimeType == typeof(String) && c.IsAlternateKey);
					if (namecol == null)
					{
						throw new MappingException(String.Concat("Model type ", typeof(TModel).Name,
																										" declares behavior EntityBehaviors.MapEnum but a column to hold the enum name cannot be determined. Specify a string column with alternate key behavior."));
					}

					var enumNames = Enum.GetNames(idcol.RuntimeType);
					var enumValues = Enum.GetValues(idcol.RuntimeType);
					for (var j = 0; j < enumNames.Length; j++)
					{
						batch.Append(Environment.NewLine)
								.Append("INSERT INTO ")
								.Append(mapping.DbObjectReference)
								.Append(" (")
								.Append(mapping.QuoteObjectName(idcol.TargetName))
								.Append(", ")
								.Append(mapping.QuoteObjectName(namecol.TargetName))
								.Append(") VALUES (")
								.Append(Convert.ToInt32(enumValues.GetValue(j)))
								.Append(", '")
								.Append(enumNames[j])
								.Append("')");
					}
				}
			}
		}

		/// <summary>
		///   Gets a model command for selecting all models of the type TModel.
		/// </summary>
		/// <returns></returns>
		public override IDataModelQueryManyCommand<TModel, SqlConnection> GetAllCommand()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Gets a create command.
		/// </summary>
		/// <returns></returns>
		public override IDataModelQuerySingleCommand<TModel, SqlConnection, TModel> GetCreateCommand()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Gets a delete (by ID) command.
		/// </summary>
		/// <returns></returns>
		public override IDataModelNonQueryCommand<TModel, SqlConnection, TIdentityKey> GetDeleteCommand()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Gets a read (by ID) command.
		/// </summary>
		/// <returns></returns>
		public override IDataModelQuerySingleCommand<TModel, SqlConnection, TIdentityKey> GetReadCommand()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Gets an update command.
		/// </summary>
		/// <returns></returns>
		public override IDataModelQuerySingleCommand<TModel, SqlConnection, TModel> GetUpdateCommand()
		{
			throw new NotImplementedException();
		}

		public override IDataModelQueryCommandBuilder<TModel, SqlConnection, TCriteria> MakeQueryCommand<TCriteria>(string queryKey, TCriteria input)
		{
			return new SqlDataModelQueryCommandBuilder<TModel, TModelImpl, TCriteria>(queryKey, _sqlWriter);
		}
		public override IDataModelQueryCommandBuilder<TModel, SqlConnection, TParam> MakeQueryCommand<TParam>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TModel, TModelImpl, TParam>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TModel, SqlConnection, TParam, TParam1> MakeQueryCommand
			<TParam, TParam1>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TModel, TModelImpl, TParam, TParam1>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TModel, SqlConnection, TParam, TParam1, TParam2> MakeQueryCommand
			<TParam, TParam1, TParam2>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TModel, TModelImpl, TParam, TParam1, TParam2>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TModel, SqlConnection, TParam, TParam1, TParam2, TParam3> MakeQueryCommand
			<TParam, TParam1, TParam2, TParam3>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TModel, TModelImpl, TParam, TParam1, TParam2, TParam3>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4> MakeQueryCommand
			<TParam, TParam1, TParam2, TParam3, TParam4>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> MakeQueryCommand
			<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> MakeQueryCommand
			<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> MakeQueryCommand
			<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> MakeQueryCommand
			<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(queryKey, _sqlWriter);
		}
		public override IDataModelCommandBuilder<TModel, SqlConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> MakeQueryCommand
			<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(string queryKey)
		{
			return new SqlDataModelQueryCommandBuilder<TModel, TModelImpl, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(queryKey, _sqlWriter);
		}

		public override void Initialize()
		{
			if (!_initialized)
			{
				_initialized = true;
			}
		}
	}
}