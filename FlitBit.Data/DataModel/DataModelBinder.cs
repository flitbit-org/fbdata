#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;

namespace FlitBit.Data.DataModel
{
    /// <summary>
    ///     Basic abstract implementation of the IModelBinder&lt;TModel, TIdentityKey> interface.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TIdentityKey"></typeparam>
    /// <typeparam name="TDbConnection"></typeparam>
    public abstract class DataModelBinder<TModel, TIdentityKey, TDbConnection> :
        IDataModelBinder<TModel, TIdentityKey, TDbConnection>
        where TDbConnection : DbConnection
    {
        readonly IMapping<TModel> _mapping;

        protected DataModelBinder(IMapping<TModel> mapping, MappingStrategy strategy)
        {
            Contract.Requires<ArgumentNullException>(mapping != null);
            _mapping = mapping;
            Strategy = strategy;
        }

        public IMapping UntypedMapping { get { return _mapping; } }

        public MappingStrategy Strategy { get; private set; }

        public abstract void BuildDdlBatch(SqlWriter batch, IList<Type> members);

        public IMapping<TModel> Mapping { get { return _mapping; } }

        /// <summary>
        ///     Gets a model command for selecting all models of the type TModel.
        /// </summary>
        /// <returns></returns>
        public abstract IDataModelQueryManyCommand<TModel, TDbConnection> GetAllCommand(
            IDataModelRepository<TModel, TIdentityKey> repository);

        /// <summary>
        ///     Gets a create command.
        /// </summary>
        /// <returns></returns>
        public abstract IDataModelQuerySingleCommand<TModel, TDbConnection, TModel> GetCreateCommand();

        /// <summary>
        ///     Gets a delete (by ID) command.
        /// </summary>
        /// <returns></returns>
        public abstract IDataModelNonQueryCommand<TModel, TDbConnection, TIdentityKey> GetDeleteCommand();

        /// <summary>
        ///     Gets a read (by ID) command.
        /// </summary>
        /// <returns></returns>
        public abstract IDataModelQuerySingleCommand<TModel, TDbConnection, TIdentityKey> GetReadCommand();

        /// <summary>
        ///     Gets an update command.
        /// </summary>
        /// <returns></returns>
        public abstract IDataModelQuerySingleCommand<TModel, TDbConnection, TModel> GetUpdateCommand();

        /// <summary>
        ///     Initializes the binder.
        /// </summary>
        public abstract void Initialize();

        protected virtual void AddGeneratorMethodsForLcgColumns(IMapping<TModel> mapping, SqlWriter sql)
        {
            var lcgColumns = mapping.Identity.Columns
                                    .Where(c => c.Column.Behaviors.HasFlag(ColumnBehaviors.LinearCongruentGenerated))
                                    .Select(c => c.Column);

            foreach (var col in lcgColumns)
            {
                sql.NewLine()
                   .Append("EXEC [SyntheticID].[GenerateSyntheticIDGenerator] ")
                   .Append(mapping.TargetSchema)
                   .Append(", '")
                   .Append(mapping.TargetObject)
                   .Append("', '")
                   .Append(col.TargetName)
                   .Append("', '")
                   .Append(col.VariableLength);
                sql.Append(col.Behaviors.HasFlag(ColumnBehaviors.LinearCongruentGeneratedAsHexidecimal)
                               ? "', 0"
                               : "', 1");
            }
        }

        protected virtual void AddIndex(IMapping<TModel> mapping, SqlWriter sql, string dbObjectName,
            string indexBaseName,
            MapIndexAttribute index, bool any)
        {
            var includedColumns = index.GetIncludedColumns(typeof(TModel));
            var columns = includedColumns as string[] ?? includedColumns.ToArray();
            if (any || (!index.Behaviors.HasFlag(IndexBehaviors.Unique)
                        || columns.Any()))
            {
                sql.NewLine()
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
                sql.NewLine()
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
                    sql.Append(mapping.QuoteObjectName(col.TargetName))
                       .Append(" ")
                       .Append(def.Order.ToString()
                                  .ToUpper());
                }
                sql.Append(")");
                if (columns.Any())
                {
                    sql.NewLine()
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
                        sql.Append(mapping.QuoteObjectName(col.TargetName));
                    }
                    sql.Append(")");
                }
            }
        }

        protected virtual void AddIndexesForTable(IMapping<TModel> mapping, SqlWriter sql)
        {
            var dbObjectName = mapping.DbObjectReference;
            var indexBaseName = String.Concat("AK_", mapping.TargetSchema, mapping.TargetObject, "_");

            foreach (MapIndexAttribute index in typeof(TModel).GetCustomAttributes(typeof(MapIndexAttribute), false))
            {
                AddIndex(mapping, sql, dbObjectName, indexBaseName, index, false);
            }
            foreach (var dep in mapping.Dependencies.Where(d => d.Kind == DependencyKind.ColumnContributor))
            {
                foreach (
                    MapIndexAttribute index in
                        dep.Target.RuntimeType.GetCustomAttributes(typeof(MapIndexAttribute), false)
                    )
                {
                    AddIndex(mapping, sql, dbObjectName, indexBaseName, index, true);
                }
            }
        }

        protected virtual void AddTableConstraintsForIndexes(IMapping<TModel> mapping, SqlWriter sql)
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
                        sql.NewLine()
                           .NewLine()
                           .Append("\t-- Table Constraints");
                    }
                    sql.NewLine()
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
                        sql.Append(mapping.QuoteObjectName(col.TargetName))
                           .Append(" ")
                           .Append(def.Order.ToString()
                                      .ToUpper());
                    }
                    sql.Append(")");
                }
            }
        }

        public abstract IDataModelRepository<TModel, TIdentityKey> MakeRepository();

        public abstract object ConstructQueryCommand(
            IDataModelRepository<TModel, TIdentityKey, TDbConnection> repo, string key,
            DataModelSqlExpression<TModel> sql,
            IDataModelWriter<TModel> writer);

        public abstract IDataModelWriter<TModel> Writer { get; }
    }
}