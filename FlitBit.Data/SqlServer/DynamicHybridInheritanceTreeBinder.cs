using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using FlitBit.Core;
using FlitBit.Data.Meta;

namespace FlitBit.Data.SqlServer
{
	public class DynamicHybridInheritanceTreeBinder<TModel, Id, TModelImpl> : IModelBinder<TModel, Id>
		where TModelImpl : class, TModel, new()
	{
		bool _initialized;
		readonly Mapping<TModel> _mapping;

		public DynamicHybridInheritanceTreeBinder(IDataModelCatalog catalog)
		{
			this._mapping = Mapping.Instance.ForType<TModel>();
			this.Catalog = catalog;
		}

		void Initialize()
		{
			if (!_initialized)
			{	
				_initialized = true;
			}
		}

		public IDataModelCatalog Catalog { get; private set; }

		public MappingStrategy Strategy
		{
			get { return MappingStrategy.DynamicHybridInheritanceTree; }
		}

		public IModelCommand<TModel, TModel, DbConnection> GetCreateCommand()
		{
			throw new NotImplementedException();
		}

		public IModelCommand<TModel, Id, DbConnection> GetReadCommand()
		{
			throw new NotImplementedException();
		}

		public IModelCommand<TModel, TModel, DbConnection> GetUpdateCommand()
		{
			throw new NotImplementedException();
		}

		public IModelCommand<TModel, Id, DbConnection> GetDeleteCommand()
		{
			throw new NotImplementedException();
		}

		public IModelCommand<TModel, DbConnection> GetAllCommand()
		{
			throw new NotImplementedException();
		}

		public IModelCommand<TModel, TMatch, DbConnection> MakeReadMatchCommand<TMatch>(TMatch match) where TMatch : class
		{
			throw new NotImplementedException();
		}

		public IModelCommand<TModel, TMatch, DbConnection> MakeUpdateMatchCommand<TMatch>(TMatch match) where TMatch : class
		{
			throw new NotImplementedException();
		}

		public IModelCommand<TModel, TMatch, DbConnection> MakeDeleteMatchCommand<TMatch>(TMatch match) where TMatch : class
		{
			throw new NotImplementedException();
		}

		public string BuildDdlBatch()
		{
			var mapping = _mapping;
			Contract.Assert(mapping.ConnectionName != null && mapping.ConnectionName.Length > 0, "ConnectionName must be set before creating SQL commands for a model");
			var sql = new StringBuilder(2000);

			var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(mapping.ConnectionName);

			var catalog = mapping.TargetCatalog;
			var schema = mapping.TargetSchema;
			var table = mapping.TargetObject;
			if (String.IsNullOrEmpty(table))
			{
				table = typeof(TModel).Name;
			}

			sql.Append("CREATE TABLE ").Append(mapping.DbObjectReference).Append(Environment.NewLine).Append('(');

			var hasLcgColumns = false;
			int i = 0;
			foreach (var col in mapping.Columns)
			{
				hasLcgColumns = hasLcgColumns || col.Behaviors.HasFlag(ColumnBehaviors.LinearCongruentGenerated);
				if (i++ > 0)
					sql.Append(',');
				var dbt = default(DbTypeTranslation);
				if (col.RuntimeType == typeof(String) && col.Behaviors.HasFlag(ColumnBehaviors.LinearCongruentGenerated))
				{
					dbt = helper.TranslateRuntimeType(typeof(SyntheticID));
				}
				else
				{
					dbt = helper.TranslateRuntimeType(col.RuntimeType);
				}
				if (dbt == null)
				{
					throw new MappingException(String.Concat("Model type '", typeof(TModel).Name, "' declares a column on property '", col.Member.Name, "' and that property has a type that cannot be mapped to a database type: ",
						col.RuntimeType.GetReadableSimpleName()));
				}
				sql.Append(Environment.NewLine).Append('\t').Append(mapping.QuoteObjectNameForSQL(col.TargetName)).Append(" ").Append(dbt.ProviderSqlTypeName);
				if (dbt.MustWriteLength(col.VariableLength, 0))
				{
					dbt.WriteLength(col.VariableLength, 0, sql);
				}

				// IDENTITY CONSTRAINTS
				if (col.IsSynthetic)
				{
					if (col.RuntimeType == typeof(Int32) || col.RuntimeType == typeof(Int64))
					{
						sql.Append(" IDENTITY(1,1)");
					}
					sql.Append(" NOT NULL");
					if (col.RuntimeType == typeof(Guid))
					{
						sql.Append(Environment.NewLine).Append("\t\tCONSTRAINT DF_").Append(mapping.TargetSchema)
							.Append(mapping.TargetObject).Append('_').Append(col.TargetName)
							.Append(" DEFAULT (NEWID())");
					}
					if ((col.Behaviors & ColumnBehaviors.LinearCongruentGeneratedWithCheckDigit) == ColumnBehaviors.LinearCongruentGeneratedWithCheckDigit)
					{
						sql.Append(',').Append(Environment.NewLine)
							.Append("\t\tCONSTRAINT CK_").Append(mapping.TargetSchema)
							.Append(mapping.TargetObject).Append('_').Append(col.TargetName)
							.Append(" CHECK ([SynthenticID].[IsValidID](").Append(mapping.QuoteObjectNameForSQL(col.TargetName)).Append(")");
					}
				}
				else
				{
					// NULL or NOT NULL
					if (!col.IsNullable)
						sql.Append(" NOT");
					sql.Append(" NULL");
				}
				if (col.IsIdentity && mapping.Identity.Columns.Count() == 1)
				{
					sql.Append(Environment.NewLine).Append("\t\tCONSTRAINT PK_")
						.Append(mapping.TargetSchema).Append(mapping.TargetObject).Append(" PRIMARY KEY");
				}

				if (col.RuntimeType == typeof(DateTime))
				{
					if (col.IsTimestampOnInsert || col.IsTimestampOnUpdate)
					{
						sql.Append(Environment.NewLine).Append("\t\tCONSTRAINT DF_").Append(mapping.TargetSchema)
							.Append(mapping.TargetObject).Append('_').Append(col.TargetName)
							.Append(" DEFAULT (GETUTCDATE())");
					}
					if (col.IsTimestampOnUpdate)
					{
						var timestampOnInsertCol = mapping.Columns.Where(c => c.IsTimestampOnInsert).FirstOrDefault();
						if (timestampOnInsertCol != null)
						{
							sql.Append(',').Append(Environment.NewLine).Append("\t\tCONSTRAINT CK_").Append(mapping.TargetSchema)
								.Append(mapping.TargetObject).Append('_').Append(col.TargetName)
								.Append(" CHECK (").Append(mapping.QuoteObjectNameForSQL(col.TargetName))
								.Append(" >= ").Append(mapping.QuoteObjectNameForSQL(timestampOnInsertCol.TargetName))
								.Append(")");
						}
					}
				}
				if (col.IsAlternateKey)
				{
					sql.Append(Environment.NewLine).Append("\t\tCONSTRAINT AK_")
						.Append(mapping.TargetSchema).Append(mapping.TargetObject).Append('_').Append(col.TargetName)
						.Append(" UNIQUE");
				}

				if (col.IsReference)
				{
					IMapping foreignM = Mapping.AccessMappingFor(col.ReferenceTargetMember.DeclaringType);
					var foreignC = foreignM.Columns.Where(c => c.Member == col.ReferenceTargetMember).Single();
					sql.Append(Environment.NewLine).Append("\t\tCONSTRAINT FK_").Append(mapping.TargetSchema)
							.Append(mapping.TargetObject).Append('_').Append(col.TargetName)
							.Append(Environment.NewLine).Append("\t\t\tFOREIGN KEY REFERENCES ")
							.Append(foreignM.DbObjectReference).Append('(').Append(foreignM.QuoteObjectNameForSQL(foreignC.TargetName)).Append(')');
					if (col.ReferenceBehaviors.HasFlag(ReferenceBehaviors.OnDeleteCascade))
					{
						sql.Append(Environment.NewLine).Append("\t\t\t\tON DELETE CASCADE");
					}
					if (foreignM.IsEnum || col.ReferenceBehaviors.HasFlag(ReferenceBehaviors.OnUpdateCascade))
					{
						sql.Append(Environment.NewLine).Append("\t\t\t\tON UPDATE CASCADE");
					}
				}
			}

			if (!mapping.Behaviors.HasFlag(EntityBehaviors.DefinedColumnsOnly))
			{
				sql.Append(',').Append(Environment.NewLine).Append(Environment.NewLine).Append("\t-- ETL Columns");
				sql.Append(Environment.NewLine).Append("\t[ETLHash] VARCHAR(32) NULL,");
				sql.Append(Environment.NewLine).Append("\t[ETLNaturalKey] VARCHAR(256) NULL,");
				sql.Append(Environment.NewLine).Append("\t[ETLCategory] VARCHAR(64) NULL");
			}
			AddTableConstraintsForIndexes(mapping, sql);
			sql.Append(Environment.NewLine).Append(')');
			AddIndexesForTable(mapping, sql);
			if (hasLcgColumns)
				AddGeneratorMethodsForLcgColumns(mapping, sql);
			if (mapping.Behaviors.HasFlag(EntityBehaviors.MapEnum))
			{
				var idenum = mapping.Identity.Columns.Where(c => c.RuntimeType.IsEnum).Single();
				if (idenum == null)
					throw new MappingException(String.Concat("Model type '", typeof(TModel).Name, "' declares behavior EntityBehaviors.MapEnum but the enum type cannot be determined. Specify an identity column of enum type."));
				var namecol = mapping.Columns.Where(c => c.RuntimeType == typeof(String) && c.IsAlternateKey).FirstOrDefault();
				if (namecol == null)
					throw new MappingException(String.Concat("Model type ", typeof(TModel).Name, " declares behavior EntityBehaviors.MapEnum but a column to hold the enum name cannot be determined. Specify a string column with alternate key behavior."));

				var enumNames = Enum.GetNames(idenum.RuntimeType);
				var enumValues = Enum.GetValues(idenum.RuntimeType);
				for (var j = 0; j < enumNames.Length; j++)
				{
					sql.Append(Environment.NewLine).Append("INSERT INTO ").Append(mapping.DbObjectReference)
						.Append(" (").Append(mapping.QuoteObjectNameForSQL(idenum.TargetName))
						.Append(", ").Append(mapping.QuoteObjectNameForSQL(namecol.TargetName)).Append(") VALUES (")
						.Append(Convert.ToInt32(enumValues.GetValue(j))).Append(", '").Append(enumNames[j]).Append("')");
				}
			}
			return sql.ToString();
		}
		private void AddGeneratorMethodsForLcgColumns(Mapping<TModel> mapping, StringBuilder sql)
		{
			var lcgColumns = mapping.Identity.Columns
				.Where(c => c.Behaviors.HasFlag(ColumnBehaviors.LinearCongruentGenerated));

			foreach (var col in lcgColumns)
			{
				sql.Append(Environment.NewLine).Append("EXEC [SyntheticID].[GenerateSyntheticIDGenerator] ")
					.Append(mapping.TargetSchema)
					.Append(", '").Append(mapping.TargetObject)
					.Append("', '").Append(col.TargetName)
					.Append("', '").Append(col.VariableLength);
				if (col.Behaviors.HasFlag(ColumnBehaviors.LinearCongruentGeneratedAsHexidecimal))
				{
					sql.Append("', 0");
				}
				else
				{
					sql.Append("', 1");
				}
			}
		}

		private void AddIndexesForTable(Mapping<TModel> mapping, StringBuilder sql)
		{
			foreach (MapIndexAttribute index in typeof(TModel).GetCustomAttributes(typeof(MapIndexAttribute), false))
			{
				var includedColumns = index.GetIncludedColumns(typeof(TModel));
				if ((index.Behaviors & IndexBehaviors.Unique) != Meta.IndexBehaviors.Unique
					|| includedColumns.Count() > 0)
				{
					sql.Append(Environment.NewLine).Append("CREATE ");
					if (index.Behaviors.HasFlag(IndexBehaviors.Unique))
					{
						sql.Append("UNIQUE ");
					}
					if (index.Behaviors.HasFlag(IndexBehaviors.Clustered))
					{
						sql.Append("CLUSTERED ");
					}
					else
					{
						sql.Append("NONCLUSTERED ");
					}
					sql.Append("INDEX ").Append("AK_").Append(mapping.TargetSchema)
						.Append(mapping.TargetObject).Append('_');
					var columnDefs = index.GetColumnSpecs(typeof(TModel));
					foreach (var def in columnDefs)
					{
						var col = mapping.Columns.Where(c => c.Member.Name == def.Column).SingleOrDefault();
						if (col == null)
							throw new MappingException(String.Concat("Index on model type ", typeof(TModel).Name, " names a property that was not found: ", def.Column));

						sql.Append(col.TargetName);
					}
					sql.Append(Environment.NewLine).Append("\tON ")
						.Append(mapping.DbObjectReference).Append(" (");

					var j = 0;
					foreach (var def in columnDefs)
					{
						var col = mapping.Columns.Where(c => c.Member.Name == def.Column).Single();
						if (j++ > 0)
							sql.Append(", ");
						sql.Append(mapping.QuoteObjectNameForSQL(col.TargetName))
							.Append(" ").Append(def.Order.ToString().ToUpper());
					}
					sql.Append(")");
					if (includedColumns.Count() > 0)
					{
						sql.Append(Environment.NewLine).Append("\tINCLUDE(");
						j = 0;
						foreach (var n in includedColumns)
						{
							var col = mapping.Columns.Where(c => c.Member.Name == n).SingleOrDefault();
							if (col == null)
								throw new MappingException(String.Concat("Index on model type ", typeof(TModel).Name, " names a property that was not found: ", n));

							if (j++ > 0) sql.Append(", ");
							sql.Append(mapping.QuoteObjectNameForSQL(col.TargetName));
						}
						sql.Append(")");
					}
				}
			}
		}
		private void AddTableConstraintsForIndexes(Mapping<TModel> mapping, StringBuilder sql)
		{
			var tableConstraints = 0;
			foreach (MapIndexAttribute index in typeof(TModel).GetCustomAttributes(typeof(MapIndexAttribute), false))
			{
				if (index.Behaviors.HasFlag(IndexBehaviors.Unique)
					&& String.IsNullOrEmpty(index.Include))
				{
					sql.Append(',');
					if (tableConstraints++ == 0)
						sql.Append(Environment.NewLine).Append(Environment.NewLine).Append("\t-- Table Constraints");
					sql.Append(Environment.NewLine).Append("\tCONSTRAINT AK_").Append(mapping.TargetSchema)
						.Append(mapping.TargetObject).Append('_');
					var columnDefs = index.GetColumnSpecs(typeof(TModel));
					foreach (var def in columnDefs)
					{
						var col = mapping.Columns.Where(c => c.Member.Name == def.Column).SingleOrDefault();
						if (col == null)
							throw new MappingException(String.Concat("Index on model ", typeof(TModel).Name, " names a property that was not found: '", def.Column, "'"));

						sql.Append(col.TargetName);
					}
					sql.Append(" UNIQUE");
					if (index.Behaviors.HasFlag(IndexBehaviors.Clustered))
						sql.Append(" CLUSTERED (");
					else
						sql.Append(" NONCLUSTERED (");

					var j = 0;
					foreach (var def in columnDefs)
					{
						var col = mapping.Columns.Where(c => c.Member.Name == def.Column).Single();
						if (j++ > 0)
							sql.Append(", ");
						sql.Append(mapping.QuoteObjectNameForSQL(col.TargetName))
							.Append(" ").Append(def.Order.ToString().ToUpper());
					}
					sql.Append(")");
				}
			}
		}

	}
}
