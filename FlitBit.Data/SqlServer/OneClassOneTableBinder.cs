using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using FlitBit.Core;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;

namespace FlitBit.Data.SqlServer
{
	/// <summary>
	///   Binds a model using the dynamic hybrid inheritance tree strategy.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TIdentityKey"></typeparam>
	/// <typeparam name="TModelImpl"></typeparam>
	public class OneClassOneTableBinder<TModel, TIdentityKey, TModelImpl> : DataModelBinder<TModel, TIdentityKey>
		where TModelImpl : class, TModel, new()
	{
		bool _initialized;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="mapping"></param>
		public OneClassOneTableBinder(Mapping<TModel> mapping)
			: base(mapping, MappingStrategy.OneClassOneTable)
		{
			Contract.Requires<ArgumentNullException>(mapping != null);
			Contract.Requires<ArgumentException>(mapping.Strategy == MappingStrategy.OneClassOneTable);
		}

		public override void BuildDDLBatch(StringBuilder batch, IList<Type> members)
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
					binder.BuildDDLBatch(batch, members);
					batch.Append(Environment.NewLine)
							.Append("GO")
							.Append(Environment.NewLine);
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

		void Initialize()
		{
			if (!_initialized)
			{
				_initialized = true;
			}
		}

		public override IDataModelQueryManyCommand<TModel, DbConnection> GetAllCommand()
		{
			throw new NotImplementedException();
		}

		public override IDataModelQuerySingleCommand<TModel, DbConnection, TModel> GetCreateCommand()
		{
			throw new NotImplementedException();
		}

		public override IDataModelNonQueryCommand<TModel, DbConnection, TIdentityKey> GetDeleteCommand()
		{
			throw new NotImplementedException();
		}

		public override IDataModelQuerySingleCommand<TModel, DbConnection, TIdentityKey> GetReadCommand()
		{
			throw new NotImplementedException();
		}

		public override IDataModelQuerySingleCommand<TModel, DbConnection, TModel> GetUpdateCommand()
		{
			throw new NotImplementedException();
		}

		public override IDataModelNonQueryCommand<TModel, DbConnection, TMatch> MakeDeleteMatchCommand<TMatch>(TMatch match)
		{
			throw new NotImplementedException();
		}

		public override IDataModelQueryManyCommand<TModel, DbConnection, TMatch> MakeReadMatchCommand<TMatch>(TMatch match)
		{
			throw new NotImplementedException();
		}

		public override IDataModelNonQueryCommand<TModel, DbConnection, TMatch, TUpdate> MakeUpdateMatchCommand<TMatch, TUpdate>(TMatch match, TUpdate update)
		{
			throw new NotImplementedException();
		}
	}
}