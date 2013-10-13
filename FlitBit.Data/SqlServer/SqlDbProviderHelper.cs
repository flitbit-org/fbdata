#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using FlitBit.Core;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;

namespace FlitBit.Data.SqlServer
{
	public class SqlDbProviderHelper : DbProviderHelper
	{
		static readonly IDbExecutable __catalogExists = DbExecutable.FromCommandText(@"
SELECT CONVERT(BIT, CASE ISNULL(DB_ID(@catalog),-1) WHEN -1 THEN 0 ELSE 1 END) AS HasCatalog"
			).DefineParameter("@catalog", DbType.String);

		static readonly IDbExecutable __schemaExists = DbExecutable.FromCommandText(@"
SELECT name AS SCHEMA_NAME 
FROM sys.schemas 
WHERE name = @schema"
			).DefineParameter("@schema", DbType.String);

		public SqlDbProviderHelper()
		{
			base.Factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
		}

		protected override void Initialize()
		{
			base.Initialize();
			MapRuntimeType<DateTime>(new SqlMappedDateTimeEmitter(SqlDbType.DateTime2));
			MapRuntimeType<Int32>(new SqlMappedInt32Emitter());
			MapRuntimeType<Int64>(new SqlMappedInt64Emitter());
			MapRuntimeType<string>(new SqlMappedStringEmitter(SqlDbType.NVarChar));
			MapRuntimeType<Type>(new SqlMappedTypeToStringEmitter());
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
			Contract.Assert(typeof(SqlConnection).IsInstanceOfType(connection), "Invalid DbConnection for DbProvider");

			var scn = (SqlConnection)connection;
			return __catalogExists.ImmediateExecuteSingleOrDefault<bool>(scn,
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

		public override IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText, CommandType cmdType)
		{
			return new SqlDbExecutable(connectionName, cmdText, cmdType);
		}

		public override IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText, CommandType cmdType,
			int cmdTimeout)
		{
			return new SqlDbExecutable(connectionName, cmdText, cmdType, cmdTimeout);
		}

		public override IDbExecutable DefineExecutableOnConnection(string connectionName, IDbExecutable exe)
		{
			return new SqlDbExecutable(connectionName, exe);
		}

		public override IDbExecutable DefineExecutableOnConnection(DbConnection connection, IDbExecutable exe)
		{
			return new SqlDbExecutable((SqlConnection)connection, exe);
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
			Contract.Assert(name != null);
			Contract.Assert(name.Length > 0);
			return (name[0] == '@') ? name : String.Concat("@", name);
		}

		public override IDataModelBinder<TModel, Id> GetModelBinder<TModel, Id>(Mapping<TModel> mapping)
		{
			Type binderType;

			switch (mapping.Strategy)
			{
				case MappingStrategy.Default:
					binderType = typeof(DynamicHybridInheritanceTreeBinder<,,>).MakeGenericType(typeof(TModel), typeof(Id),
																																													Mapping<TModel>.Instance.ConcreteType);
					break;
				case MappingStrategy.OneClassOneTable:
					binderType = typeof(OneClassOneTableBinder<,,>).MakeGenericType(typeof(TModel), typeof(Id),
																																													Mapping<TModel>.Instance.ConcreteType);
					break;
				case MappingStrategy.OneInheritanceTreeOneTable:
					throw new NotImplementedException();
				case MappingStrategy.OneInheritancePathOneTable:
					throw new NotImplementedException();
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			var result = (IDataModelBinder<TModel, Id>)Activator.CreateInstance(binderType, mapping);
			result.Initialize();
			return result;
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

		public override IDataParameterBinder MakeParameterBinder()
		{
			return new SqlParameterBinder();
		}

		public override IDataParameterBinder MakeParameterBinder(DbCommand cmd)
		{
			return new DirectSqlParameterBinder((SqlCommand)cmd);
		}

		public override bool SchemaExists(DbConnection connection, string catalog, string schema)
		{
			Contract.Assert(typeof(SqlConnection).IsInstanceOfType(connection), "Invalid DbConnection for DbProvider");

			var scn = (SqlConnection)connection;
			if (!String.Equals(scn.Database, catalog, StringComparison.OrdinalIgnoreCase))
			{
				scn.ChangeDatabase(catalog);
			}
			return __schemaExists.ImmediateExecuteSingleOrDefault<bool>(scn,
																																	b => b.SetParameterValue("@schema", catalog),
																																	r => !r.IsDBNull(r.GetOrdinal("SCHEMA_NAME"))
				);
		}

		public override bool SupportsAsynchronousProcessing(DbConnection connection)
		{
			Contract.Assert(typeof(SqlConnection).IsInstanceOfType(connection), "Invalid DbConnection for DbProvider");

			var scn = (SqlConnection)connection;
			var cnsb = new SqlConnectionStringBuilder(scn.ConnectionString);
			return cnsb.AsynchronousProcessing;
		}

		public override bool SupportsMultipleActiveResultSets(DbConnection connection)
		{
			Contract.Assert(typeof(SqlConnection).IsInstanceOfType(connection), "Invalid DbConnection for DbProvider");

			var scn = (SqlConnection)connection;
			var cnsb = new SqlConnectionStringBuilder(scn.ConnectionString);
			return cnsb.MultipleActiveResultSets;
		}

		public override DbTypeTranslation TranslateRuntimeType(Type type)
		{
			return SqlDbTypeTranslations.TranslateRuntimeType(type);
		}

		protected override string FormatCreateSchemaCommandText(string catalog, string schema)
		{
			// SQL Server doesn't allow you to specify the catalog, it assumes current catalog
			// It also requires that CREATE SCHEMA is the first command in a batch.
			return String.Concat("USE [", catalog, "] EXECUTE ('CREATE SCHEMA [", schema, "]')");
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
					.AppendLine("END")
					.AppendLine("GO")
					;
		}
	}
}