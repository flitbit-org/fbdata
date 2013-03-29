﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using FlitBit.Core;
using FlitBit.Data.Meta;

namespace FlitBit.Data.SqlServer
{
	/// <summary>
	/// Binds a model using the dynamic hybrid inheritance tree strategy.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TIdentityKey"></typeparam>
	/// <typeparam name="TModelImpl"></typeparam>
	public class DynamicHybridInheritanceTreeBinder<TModel, TIdentityKey, TModelImpl> : IModelBinder<TModel, TIdentityKey>
		where TModelImpl : class, TModel, new()
	{
		readonly Mapping<TModel> _mapping;
		bool _initialized;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="mapping"></param>
		public DynamicHybridInheritanceTreeBinder(Mapping<TModel> mapping)
		{
			Contract.Requires<ArgumentNullException>(mapping != null);
			//Contract.Requires<ArgumentException>(mapping.IsComplete);
			Contract.Requires<ArgumentException>(mapping.Strategy == MappingStrategy.DynamicHybridInheritanceTree);

			this._mapping = mapping;
		}

		void AddGeneratorMethodsForLcgColumns(Mapping<TModel> mapping, StringBuilder sql)
		{
			var lcgColumns = mapping.Identity.Columns
															.Where(c => c.Behaviors.HasFlag(ColumnBehaviors.LinearCongruentGenerated));

			foreach (var col in lcgColumns)
			{
				sql.Append(Environment.NewLine)
					.Append("EXEC [SyntheticID].[GenerateSyntheticIDGenerator] ")
					.Append(mapping.TargetSchema)
					.Append(", '")
					.Append(mapping.TargetObject)
					.Append("', '")
					.Append(col.TargetName)
					.Append("', '")
					.Append(col.VariableLength);
				sql.Append(col.Behaviors.HasFlag(ColumnBehaviors.LinearCongruentGeneratedAsHexidecimal) ? "', 0" : "', 1");
			}
		}

		void AddIndex(Mapping<TModel> mapping, StringBuilder sql, string dbObjectName, string indexBaseName,
			MapIndexAttribute index, bool any)
		{
			var includedColumns = index.GetIncludedColumns(typeof(TModel));
			var columns = includedColumns as string[] ?? includedColumns.ToArray();
			if (any || (!index.Behaviors.HasFlag(IndexBehaviors.Unique)
				|| columns.Any()))
			{
				sql.Append(Environment.NewLine)
					.Append("CREATE ");
				if (index.Behaviors.HasFlag(IndexBehaviors.Unique))
				{
					sql.Append("UNIQUE ");
				}
				sql.Append(index.Behaviors.HasFlag(IndexBehaviors.Clustered) ? "CLUSTERED " : "NONCLUSTERED ");
				sql.Append("INDEX ")
					.Append(indexBaseName);
				var columnDefs = index.GetColumnSpecs(typeof(TModel));
				var columnSpecs = columnDefs as IndexColumnSpec[] ?? columnDefs.ToArray();
				foreach (var def in columnSpecs)
				{
					var col = mapping.Columns.SingleOrDefault(c => c.Member.Name == def.Column);
					if (col == null)
					{
						throw new MappingException(String.Concat("Index on model type ", typeof(TModel).Name,
																										" names a property that was not found: ", def.Column));
					}

					sql.Append(col.TargetName);
				}
				sql.Append(Environment.NewLine)
					.Append("\tON ")
					.Append(dbObjectName)
					.Append(" (");

				var j = 0;
				foreach (var def in columnSpecs)
				{
					var col = mapping.Columns.Single(c => c.Member.Name == def.Column);
					if (j++ > 0)
					{
						sql.Append(", ");
					}
					sql.Append(mapping.QuoteObjectNameForSQL(col.TargetName))
						.Append(" ")
						.Append(def.Order.ToString()
											.ToUpper());
				}
				sql.Append(")");
				if (columns.Any())
				{
					sql.Append(Environment.NewLine)
						.Append("\tINCLUDE(");
					j = 0;
					foreach (var n in columns)
					{
						var col = mapping.Columns.SingleOrDefault(c => c.Member.Name == n);
						if (col == null)
						{
							throw new MappingException(String.Concat("Index on model type ", typeof(TModel).Name,
																											" names a property that was not found: ", n));
						}

						if (j++ > 0)
						{
							sql.Append(", ");
						}
						sql.Append(mapping.QuoteObjectNameForSQL(col.TargetName));
					}
					sql.Append(")");
				}
			}
		}

		void AddIndexesForTable(Mapping<TModel> mapping, StringBuilder sql)
		{
			var dbObjectName = mapping.DbObjectReference;
			var indexBaseName = String.Concat("AK_", mapping.TargetSchema, mapping.TargetObject, "_");

			foreach (MapIndexAttribute index in typeof(TModel).GetCustomAttributes(typeof(MapIndexAttribute), false))
			{
				AddIndex(mapping, sql, dbObjectName, indexBaseName, index, false);
			}
			foreach (var dep in mapping.Dependencies.Where(d => d.Kind == DependencyKind.ColumnContributor))
			{
				foreach (MapIndexAttribute index in dep.Target.RuntimeType.GetCustomAttributes(typeof(MapIndexAttribute), false))
				{
					AddIndex(mapping, sql, dbObjectName, indexBaseName, index, true);
				}
			}
		}

		void AddTableConstraintsForIndexes(Mapping<TModel> mapping, StringBuilder sql)
		{
			var tableConstraints = 0;
			foreach (MapIndexAttribute index in typeof(TModel).GetCustomAttributes(typeof(MapIndexAttribute), false))
			{
				if (index.Behaviors.HasFlag(IndexBehaviors.Unique)
					&& String.IsNullOrEmpty(index.Include))
				{
					sql.Append(',');
					if (tableConstraints++ == 0)
					{
						sql.Append(Environment.NewLine)
							.Append(Environment.NewLine)
							.Append("\t-- Table Constraints");
					}
					sql.Append(Environment.NewLine)
						.Append("\tCONSTRAINT AK_")
						.Append(mapping.TargetSchema)
						.Append(mapping.TargetObject)
						.Append('_');
					var columnDefs = index.GetColumnSpecs(typeof(TModel));
					var columnSpecs = columnDefs as IndexColumnSpec[] ?? columnDefs.ToArray();
					foreach (var def in columnSpecs)
					{
						var col = mapping.Columns.SingleOrDefault(c => c.Member.Name == def.Column);
						if (col == null)
						{
							throw new MappingException(String.Concat("Index on model ", typeof(TModel).Name,
																											" names a property that was not found: '", def.Column, "'"));
						}

						sql.Append(col.TargetName);
					}
					sql.Append(" UNIQUE");
					sql.Append(index.Behaviors.HasFlag(IndexBehaviors.Clustered) ? " CLUSTERED (" : " NONCLUSTERED (");

					var j = 0;
					foreach (var def in columnSpecs)
					{
						var col = mapping.Columns.Single(c => c.Member.Name == def.Column);
						if (j++ > 0)
						{
							sql.Append(", ");
						}
						sql.Append(mapping.QuoteObjectNameForSQL(col.TargetName))
							.Append(" ")
							.Append(def.Order.ToString()
												.ToUpper());
					}
					sql.Append(")");
				}
			}
		}

		void Initialize()
		{
			if (!_initialized)
			{
				_initialized = true;
			}
		}

		#region IModelBinder<TModel,Id> Members

		public IMapping Mapping { get { return _mapping; } }

		public MappingStrategy Strategy { get { return MappingStrategy.DynamicHybridInheritanceTree; } }

		public IModelCommand<TModel, TModel, DbConnection> GetCreateCommand()
		{
			throw new NotImplementedException();
		}

		public IModelCommand<TModel, TIdentityKey, DbConnection> GetReadCommand()
		{
			throw new NotImplementedException();
		}

		public IModelCommand<TModel, TModel, DbConnection> GetUpdateCommand()
		{
			throw new NotImplementedException();
		}

		public IModelCommand<TModel, TIdentityKey, DbConnection> GetDeleteCommand()
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

		public void BuildDdlBatch(StringBuilder batch, IList<Type> members)
		{
			if (!members.Contains(_mapping.RuntimeType))
			{
				members.Add(_mapping.RuntimeType);
				var mapping = _mapping;
				Contract.Assert(!string.IsNullOrEmpty(mapping.ConnectionName),
												"ConnectionName must be set before creating SQL commands for a model");

				var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(mapping.ConnectionName);
				batch.Append("-- BEGIN: ")
						.Append(typeof(TModel).GetReadableSimpleName())
						.Append(Environment.NewLine);

				foreach (
					var dep in mapping.Dependencies.Where(d => d.Kind == DependencyKind.Base || d.Kind.HasFlag(DependencyKind.Direct)))
				{
					var dmap = Mappings.AccessMappingFor(dep.Target.RuntimeType);
					var binder = dmap.GetBinder();
					binder.BuildDdlBatch(batch, members);
				}

				var catalog = mapping.TargetCatalog;
				var schema = mapping.TargetSchema;
				var table = mapping.TargetObject;
				if (String.IsNullOrEmpty(table))
				{
					table = typeof(TModel).Name;
				}

				batch.Append("CREATE TABLE ")
						.Append(mapping.DbObjectReference)
						.Append(Environment.NewLine)
						.Append('(');

				var hasLcgColumns = false;
				var i = 0;
				foreach (var col in mapping.Identity.Columns)
				{
					if (i++ > 0)
					{
						batch.Append(',');
					}
					DbTypeTranslation dbt;
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
						throw new MappingException(String.Concat("Model type '", typeof(TModel).Name, "' declares a column on property '",
																										col.Member.Name, "' and the property's type cannot be mapped to a database type: ",
																										col.RuntimeType.GetReadableSimpleName()));
					}
					batch.Append(Environment.NewLine)
							.Append('\t')
							.Append(mapping.QuoteObjectNameForSQL(col.TargetName))
							.Append(" ")
							.Append(dbt.ProviderSqlTypeName);
					if (dbt.MustWriteLength(col.VariableLength, 0))
					{
						dbt.WriteLength(col.VariableLength, 0, batch);
					}

					if (col.Member.DeclaringType == mapping.RuntimeType)
					{
						hasLcgColumns = hasLcgColumns || col.Behaviors.HasFlag(ColumnBehaviors.LinearCongruentGenerated);
						// IDENTITY CONSTRAINTS
						if (col.IsSynthetic)
						{
							if (col.RuntimeType == typeof(Int32) || col.RuntimeType == typeof(Int64))
							{
								batch.Append(" IDENTITY(1,1)");
							}
							batch.Append(" NOT NULL");
							if (col.RuntimeType == typeof(Guid))
							{
								batch.Append(Environment.NewLine)
										.Append("\t\tCONSTRAINT DF_")
										.Append(mapping.TargetSchema)
										.Append(mapping.TargetObject)
										.Append('_')
										.Append(col.TargetName)
										.Append(" DEFAULT (NEWID())");
							}
							if ((col.Behaviors & ColumnBehaviors.LinearCongruentGeneratedWithCheckDigit) ==
								ColumnBehaviors.LinearCongruentGeneratedWithCheckDigit)
							{
								batch.Append(',')
										.Append(Environment.NewLine)
										.Append("\t\tCONSTRAINT CK_")
										.Append(mapping.TargetSchema)
										.Append(mapping.TargetObject)
										.Append('_')
										.Append(col.TargetName)
										.Append(" CHECK ([SynthenticID].[IsValidID](")
										.Append(mapping.QuoteObjectNameForSQL(col.TargetName))
										.Append(")");
							}
						}
						if (mapping.Identity.Columns.Count() == 1)
						{
							batch.Append(Environment.NewLine)
									.Append("\t\tCONSTRAINT PK_")
									.Append(mapping.TargetSchema)
									.Append(mapping.TargetObject)
									.Append(" PRIMARY KEY");
						}
					}
					else
					{
						if (mapping.Identity.Columns.Count() == 1)
						{
							batch.Append(" NOT NULL")
									.Append(Environment.NewLine)
									.Append("\t\tCONSTRAINT PK_")
									.Append(mapping.TargetSchema)
									.Append(mapping.TargetObject)
									.Append(" PRIMARY KEY");

							var foreignM = col.Mapping;
							var foreignC = foreignM.GetPreferredReferenceColumn();
							batch.Append(Environment.NewLine)
									.Append("\t\tCONSTRAINT FK_")
									.Append(mapping.TargetSchema)
									.Append(mapping.TargetObject)
									.Append('_')
									.Append(col.TargetName)
									.Append(Environment.NewLine)
									.Append("\t\t\tFOREIGN KEY REFERENCES ")
									.Append(foreignM.DbObjectReference)
									.Append('(')
									.Append(foreignM.QuoteObjectNameForSQL(foreignC.TargetName))
									.Append(')')
									.Append(Environment.NewLine)
									.Append("\t\t\t\tON DELETE CASCADE")
									.Append(Environment.NewLine)
									.Append("\t\t\t\tON UPDATE CASCADE");
						}
					}
				}
				foreach (var col in mapping.DeclaredColumns.Where(c => c.IsIdentity == false))
				{
					hasLcgColumns = hasLcgColumns || col.Behaviors.HasFlag(ColumnBehaviors.LinearCongruentGenerated);
					if (i++ > 0)
					{
						batch.Append(',');
					}
					DbTypeTranslation dbt;
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
						throw new MappingException(String.Concat("Model type '", typeof(TModel).Name, "' declares a column on property '",
																										col.Member.Name, "' and the property's type cannot be mapped to a database type: ",
																										col.RuntimeType.GetReadableSimpleName()));
					}
					batch.Append(Environment.NewLine)
							.Append('\t')
							.Append(mapping.QuoteObjectNameForSQL(col.TargetName))
							.Append(" ")
							.Append(dbt.ProviderSqlTypeName);
					if (dbt.MustWriteLength(col.VariableLength, 0))
					{
						dbt.WriteLength(col.VariableLength, 0, batch);
					}

					// NULL or NOT NULL
					if (!col.IsNullable)
					{
						batch.Append(" NOT");
					}
					batch.Append(" NULL");

					if (col.RuntimeType == typeof(DateTime))
					{
						if (col.IsTimestampOnInsert || col.IsTimestampOnUpdate)
						{
							batch.Append(Environment.NewLine)
									.Append("\t\tCONSTRAINT DF_")
									.Append(mapping.TargetSchema)
									.Append(mapping.TargetObject)
									.Append('_')
									.Append(col.TargetName)
									.Append(" DEFAULT (GETUTCDATE())");
						}
						if (col.IsTimestampOnUpdate)
						{
							var timestampOnInsertCol = mapping.Columns.FirstOrDefault(c => c.IsTimestampOnInsert);
							if (timestampOnInsertCol != null)
							{
								batch.Append(',')
										.Append(Environment.NewLine)
										.Append("\t\tCONSTRAINT CK_")
										.Append(mapping.TargetSchema)
										.Append(mapping.TargetObject)
										.Append('_')
										.Append(col.TargetName)
										.Append(" CHECK (")
										.Append(mapping.QuoteObjectNameForSQL(col.TargetName))
										.Append(" >= ")
										.Append(mapping.QuoteObjectNameForSQL(timestampOnInsertCol.TargetName))
										.Append(")");
							}
						}
					}
					if (col.IsAlternateKey)
					{
						batch.Append(Environment.NewLine)
								.Append("\t\tCONSTRAINT AK_")
								.Append(mapping.TargetSchema)
								.Append(mapping.TargetObject)
								.Append('_')
								.Append(col.TargetName)
								.Append(" UNIQUE");
					}

					if (col.IsReference)
					{
						var foreignM = Mappings.AccessMappingFor(col.ReferenceTargetMember.DeclaringType);
						var foreignC = foreignM.Columns.Single(c => c.Member == col.ReferenceTargetMember);
						batch.Append(Environment.NewLine)
								.Append("\t\tCONSTRAINT FK_")
								.Append(mapping.TargetSchema)
								.Append(mapping.TargetObject)
								.Append('_')
								.Append(col.TargetName)
								.Append(Environment.NewLine)
								.Append("\t\t\tFOREIGN KEY REFERENCES ")
								.Append(foreignM.DbObjectReference)
								.Append('(')
								.Append(foreignM.QuoteObjectNameForSQL(foreignC.TargetName))
								.Append(')');
						if (col.ReferenceBehaviors.HasFlag(ReferenceBehaviors.OnDeleteCascade))
						{
							batch.Append(Environment.NewLine)
									.Append("\t\t\t\tON DELETE CASCADE");
						}
						if (foreignM.IsEnum || col.ReferenceBehaviors.HasFlag(ReferenceBehaviors.OnUpdateCascade))
						{
							batch.Append(Environment.NewLine)
									.Append("\t\t\t\tON UPDATE CASCADE");
						}
					}
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
				if (hasLcgColumns)
				{
					AddGeneratorMethodsForLcgColumns(mapping, batch);
				}
				if (mapping.Behaviors.HasFlag(EntityBehaviors.MapEnum))
				{
					var idenum = mapping.Identity.Columns.Single(c => c.RuntimeType.IsEnum);
					if (idenum == null)
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

					var enumNames = Enum.GetNames(idenum.RuntimeType);
					var enumValues = Enum.GetValues(idenum.RuntimeType);
					for (var j = 0; j < enumNames.Length; j++)
					{
						batch.Append(Environment.NewLine)
								.Append("INSERT INTO ")
								.Append(mapping.DbObjectReference)
								.Append(" (")
								.Append(mapping.QuoteObjectNameForSQL(idenum.TargetName))
								.Append(", ")
								.Append(mapping.QuoteObjectNameForSQL(namecol.TargetName))
								.Append(") VALUES (")
								.Append(Convert.ToInt32(enumValues.GetValue(j)))
								.Append(", '")
								.Append(enumNames[j])
								.Append("')");
					}
				}
				batch.Append(Environment.NewLine)
						.Append("GO")
						.Append(Environment.NewLine)
						.Append("-- END: ")
						.Append(typeof(TModel).GetReadableSimpleName())
						.Append(Environment.NewLine);
			}
		}



		#endregion
	}
}