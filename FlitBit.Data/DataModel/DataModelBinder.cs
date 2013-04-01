using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using FlitBit.Data.Meta;

namespace FlitBit.Data.DataModel
{
	/// <summary>
	/// Basic abstract implementation of the IModelBinder&lt;TModel, TIdentityKey> interface.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TIdentityKey"></typeparam>
	public abstract class DataModelBinder<TModel, TIdentityKey> : IDataModelBinder<TModel, TIdentityKey>
	{
		readonly Mapping<TModel> _mapping;

		protected DataModelBinder(Mapping<TModel> mapping, MappingStrategy strategy)
		{
			Contract.Requires<ArgumentNullException>(mapping != null);
			this._mapping = mapping;
			this.Strategy = strategy;
		}

		protected virtual void AddGeneratorMethodsForLcgColumns(Mapping<TModel> mapping, StringBuilder sql)
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

		protected virtual void AddIndex(Mapping<TModel> mapping, StringBuilder sql, string dbObjectName, string indexBaseName,
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

		protected virtual void AddIndexesForTable(Mapping<TModel> mapping, StringBuilder sql)
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

		protected virtual void AddTableConstraintsForIndexes(Mapping<TModel> mapping, StringBuilder sql)
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

		#region IModelBinder<TModel,TIdentityKey> Members

		public abstract IDataModelCommand<TModel, DbConnection> GetAllCommand();

		public abstract IDataModelCommand<TModel, TModel, DbConnection> GetCreateCommand();

		public abstract IDataModelCommand<TModel, TIdentityKey, DbConnection> GetDeleteCommand();

		public abstract IDataModelCommand<TModel, TIdentityKey, DbConnection> GetReadCommand();

		public abstract IDataModelCommand<TModel, TModel, DbConnection> GetUpdateCommand();

		public abstract IDataModelCommand<TModel, TMatch, DbConnection> MakeDeleteMatchCommand<TMatch>(TMatch match)
			where TMatch : class;

		public abstract IDataModelCommand<TModel, TMatch, DbConnection> MakeReadMatchCommand<TMatch>(TMatch match)
			where TMatch : class;

		public abstract IDataModelCommand<TModel, TMatch, DbConnection> MakeUpdateMatchCommand<TMatch>(TMatch match)
			where TMatch : class;

		public IMapping UntypedMapping { get { return this._mapping; } }

		public MappingStrategy Strategy { get; private set; }

		public abstract void BuildDDLBatch(StringBuilder batch, IList<Type> members);

		public Mapping<TModel> Mapping { get { return this._mapping; } }

		#endregion
	}
}