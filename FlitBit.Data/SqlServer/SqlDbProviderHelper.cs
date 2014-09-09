#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;

namespace FlitBit.Data.SqlServer
{
    /// <summary>
    ///     Helper for SqlConnection and SqlProviderFactory.
    /// </summary>
    public class SqlDbProviderHelper : DbProviderHelper
    {
        static readonly IDbExecutable __CatalogExists = DbExecutable.FromCommandText(
            @"SELECT CONVERT(BIT, CASE ISNULL(DB_ID(@catalog),-1) WHEN -1 THEN 0 ELSE 1 END) AS HasCatalog")
                                                                    .DefineParameter("@catalog", DbType.String);

        bool _initialized;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public SqlDbProviderHelper()
            : base(typeof(SqlConnection))
        {
            Factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        }

        public override void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            base.Initialize();
            MapRuntimeType<bool>(new SqlMappedBoolAsBitEmitter());
            MapRuntimeType<bool?>(new SqlMappedNullableBoolAsBitEmitter());
            MapRuntimeType<double>(new SqlMappedDoubleEmitter());
            MapRuntimeType<double?>(new SqlMappedNullableDoubleEmitter());
            MapRuntimeType<float>(new SqlMappedSingleEmitter());
            MapRuntimeType<float?>(new SqlMappedNullableSingleEmitter());
            MapRuntimeType<decimal>(new SqlMappedDecimalEmitter());
            MapRuntimeType<decimal?>(new SqlMappedNullableDecimalEmitter());

            MapRuntimeType<DateTime>(new SqlMappedDateTimeEmitter(SqlDbType.DateTime2));
            MapRuntimeType<DateTime?>(new SqlMappedNullableDateTimeEmitter(SqlDbType.DateTime2));
            MapRuntimeType<Int16>(new SqlMappedInt16Emitter());
            MapRuntimeType<Int16?>(new SqlMappedNullableInt16Emitter());
            MapRuntimeType<Int32>(new SqlMappedInt32Emitter());
            MapRuntimeType<Int32?>(new SqlMappedNullableInt32Emitter());
            MapRuntimeType<Int64>(new SqlMappedInt64Emitter());
            MapRuntimeType<Int64?>(new SqlMappedNullableInt64Emitter());
            MapRuntimeType<string>(new SqlMappedStringEmitter(SqlDbType.NVarChar));
            MapRuntimeType<Type>(new SqlMappedTypeToStringEmitter());
            _initialized = true;
        }

        public override IAsyncResult BeginExecuteNonQuery(DbCommand command, AsyncCallback callback, object stateObject)
        {
            var sql = (SqlCommand)command;
            return sql.BeginExecuteNonQuery(callback, stateObject);
        }

        public override IAsyncResult BeginExecuteReader(DbCommand command, AsyncCallback callback, object stateObject)
        {
            var sql = (SqlCommand)command;
            return sql.BeginExecuteReader(callback, stateObject, CommandBehavior.CloseConnection);
        }

        public override bool CanRetryTransaction(Exception ex)
        {
            var seq = ex as SqlException;
            return (seq != null && seq.Message.StartsWith("|1205|"));
        }

        public override bool CatalogExists(DbConnection connection, string catalog)
        {
            Contract.Requires<ArgumentException>(connection is SqlConnection, "Invalid DbConnection for DbProvider");

            var scn = (SqlConnection)connection;
            return __CatalogExists.ImmediateExecuteSingleOrDefault(scn,
                b => b.SetParameterValue("@catalog", catalog),
                r => r.GetBoolean(r.GetOrdinal("HasCatalog"))
                );
        }

        public override IDbExecutable DefineExecutableOnConnection(string connectionName)
        {
            return new SqlDbExecutable(connectionName, default(String));
        }

        public override IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText)
        {
            return new SqlDbExecutable(connectionName, cmdText);
        }

        public override IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText,
            CommandType cmdType)
        {
            return new SqlDbExecutable(connectionName, cmdText, cmdType);
        }

        public override IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText,
            CommandType cmdType,
            int cmdTimeout)
        {
            return new SqlDbExecutable(connectionName, cmdText, cmdType, cmdTimeout);
        }

        public override IDbExecutable DefineExecutableOnConnection(string connectionName, IDbExecutable definition)
        {
            return new SqlDbExecutable(connectionName, definition);
        }

        public override IDbExecutable DefineExecutableOnConnection(DbConnection connection, IDbExecutable definition)
        {
            return new SqlDbExecutable((SqlConnection)connection, definition);
        }

        public override IDbExecutable DefineExecutableOnConnection(DbConnection connection, string cmdText)
        {
            return new SqlDbExecutable((SqlConnection)connection, cmdText);
        }

        public override IDbExecutable DefineExecutableOnConnection(DbConnection connection, string cmdText,
            CommandType cmdType)
        {
            return new SqlDbExecutable((SqlConnection)connection, cmdText, cmdType);
        }

        public override IDbExecutable DefineExecutableOnConnection(DbConnection connection, string cmdText,
            CommandType cmdType, int cmdTimeout)
        {
            return new SqlDbExecutable((SqlConnection)connection, cmdText, cmdType, cmdTimeout);
        }

        public override int EndExecuteNonQuery(DbCommand command, IAsyncResult ar)
        {
            var sql = (SqlCommand)command;
            return sql.EndExecuteNonQuery(ar);
        }

        public override DbDataReader EndExecuteReader(DbCommand command, IAsyncResult ar)
        {
            var sql = (SqlCommand)command;
            return sql.EndExecuteReader(ar);
        }

        public override string FormatParameterName(string name)
        {
            return (name[0] == '@') ? name : String.Concat("@", name);
        }

        public override IDataModelBinder<TModel, TIdentityKey> GetModelBinder<TModel, TIdentityKey>(
            IMapping<TModel> mapping)
        {
            Type binderType;
            var concrete = DataModel<TModel>.ConcreteType;

            switch (mapping.Strategy)
            {
                case MappingStrategy.Default:
                    binderType = typeof(DynamicHybridInheritanceTreeBinder<,,>).MakeGenericType(typeof(TModel),
                        typeof(TIdentityKey),
                        concrete);
                    break;
                case MappingStrategy.OneClassOneTable:
                    binderType = typeof(OneClassOneTableBinder<,,>).MakeGenericType(typeof(TModel), typeof(TIdentityKey),
                        concrete);
                    break;
                case MappingStrategy.OneInheritanceTreeOneTable:
                    throw new NotImplementedException();
                case MappingStrategy.OneInheritancePathOneTable:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var result = (IDataModelBinder<TModel, TIdentityKey>)Activator.CreateInstance(binderType, mapping);
            result.Initialize();
            return result;
        }

        protected override Type MakeEnumAsInt16Emitter(Type enumType)
        {
            return typeof(SqlMappedEmumAsInt16Emitter<>).MakeGenericType(enumType);
        }

        protected override Type MakeEnumAsInt32Emitter(Type enumType)
        {
            return typeof(SqlMappedEmumAsInt32Emitter<>).MakeGenericType(enumType);
        }

        public override string GetServerName(DbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            return connection.ImmediateExecuteEnumerable("SELECT @@SERVERNAME")
                             .Single()
                             .GetString(0);
        }

        public override IDataParameterBinder MakeParameterBinder() { return new SqlParameterBinder(); }

        public override IDataParameterBinder MakeParameterBinder(DbCommand cmd)
        {
            return new DirectSqlParameterBinder((SqlCommand)cmd);
        }

        /// <summary>
        ///     Determins if the specified catalog and schema exist on the specified connection.
        /// </summary>
        /// <param name="connection">the connection</param>
        /// <param name="catalog">the catalog name</param>
        /// <param name="schema">the schema name</param>
        /// <returns><em>true</em> if the schema exists in the specified catalog; otherwise <em>false</em></returns>
        protected override bool PerformSchemaExists(DbConnection connection, string catalog, string schema)
        {
            var scn = (SqlConnection)connection;
            string restoreCatalog = null;
            if (!String.Equals(scn.Database, catalog, StringComparison.OrdinalIgnoreCase))
            {
                restoreCatalog = scn.Database;
                scn.ChangeDatabase(catalog);
            }

            var res = base.PerformSchemaExists(connection, catalog, schema);
            if (restoreCatalog != null)
            {
                scn.ChangeDatabase(restoreCatalog);
            }
            return res;
        }

        /// <summary>
        ///     When implemented by subclass; creates the specified schema in the specified catalog.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="catalog"></param>
        /// <param name="schema"></param>
        protected override void PerformCreateSchema(DbConnection connection, string catalog, string schema)
        {
            // SQL Server doesn't allow you to specify the catalog, it assumes current catalog
            // It also requires that CREATE SCHEMA is the first command in a batch.
            ExecuteNonQuery(connection,
                cmd =>
                cmd.CommandText =
                String.Concat("USE [", catalog, "] EXECUTE ('CREATE SCHEMA [", schema, "]')"),
                null
                );
        }

        public override bool SupportsAsynchronousProcessing(DbConnection connection)
        {
            Contract.Assert(connection is SqlConnection, "Invalid DbConnection for DbProvider");

            var scn = (SqlConnection)connection;
            var cnsb = new SqlConnectionStringBuilder(scn.ConnectionString);
            return cnsb.AsynchronousProcessing;
        }

        public override bool SupportsMultipleActiveResultSets(DbConnection connection)
        {
            Contract.Assert(connection is SqlConnection, "Invalid DbConnection for DbProvider");

            var scn = (SqlConnection)connection;
            var cnsb = new SqlConnectionStringBuilder(scn.ConnectionString);
            return cnsb.MultipleActiveResultSets;
        }

        public override DbTypeTranslation TranslateRuntimeType(Type type)
        {
            return SqlDbTypeTranslations.TranslateRuntimeType(type);
        }

        public override void EmitCreateSchema(StringBuilder batch, string schemaName)
        {
            batch.Append(Environment.NewLine)
                 .Append("IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = '")
                 .Append(schemaName)
                 .Append("')")
                 .Append(Environment.NewLine)
                 .AppendLine("BEGIN")
                 .Append("\tEXEC( 'CREATE SCHEMA [").Append(schemaName).Append("]' )")
                 .Append(Environment.NewLine)
                 .AppendLine("END;")
                ;
        }
    }
}