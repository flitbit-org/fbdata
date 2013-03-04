﻿#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using FlitBit.Core.Properties;

namespace FlitBit.Data.SqlServer
{																																					
	public class SqlDbProviderHelper : DbProviderHelper
	{
		static readonly IDbExecutable __catalogExists = DbExecutable.FromCommandText(@"SELECT CONVERT(BIT, CASE ISNULL(DB_ID(@catalog),-1) WHEN -1 THEN 0 ELSE 1 END) AS HasCatalog")
			.DefineParameter("@catalog", DbType.String);

		static readonly IDbExecutable __schemaExists = DbExecutable.FromCommandText(@"
SELECT name AS SCHEMA_NAME 
FROM sys.schemas 
WHERE name = @schema")
			.DefineParameter("@schema", DbType.String);

		public SqlDbProviderHelper()
		{
			base.Factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
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
						
		public override bool SupportsMultipleActiveResultSets(DbConnection connection)
		{
			Contract.Assert(typeof(SqlConnection).IsInstanceOfType(connection), "Invalid DbConnection for DbProvider");

			SqlConnection scn = (SqlConnection)connection;
			SqlConnectionStringBuilder cnsb = new SqlConnectionStringBuilder(scn.ConnectionString);
			return cnsb.MultipleActiveResultSets;
		}

		public override bool SupportsAsynchronousProcessing(DbConnection connection)
		{
			Contract.Assert(typeof(SqlConnection).IsInstanceOfType(connection), "Invalid DbConnection for DbProvider");

			SqlConnection scn = (SqlConnection)connection;
			SqlConnectionStringBuilder cnsb = new SqlConnectionStringBuilder(scn.ConnectionString);
			return cnsb.AsynchronousProcessing;
		}
				
		public override string FormatParameterName(string name)
		{
			Contract.Assert(name != null);
			Contract.Assert(name.Length > 0);
			return (name[0] == '@') ? name : String.Concat("@", name);
		}
				
		public override string GetServerName(DbConnection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			return connection.ImmediateExecuteEnumerable("SELECT @@SERVERNAME").Single().GetString(0);
		}

		protected override string FormatCreateSchemaCommandText(string catalog, string schema)
		{
			// SQL Server doesn't allow you to specify the catalog, it assumes current catalog
			// It also requires that CREATE SCHEMA is the first command in a batch.
			return String.Concat("USE [", catalog, "] EXECUTE ('CREATE SCHEMA [", schema, "]')");
		}

		public override bool CanRetryTransaction(Exception ex)
		{
			SqlException seq = ex as SqlException;
			return (seq != null && seq.Message.StartsWith("|1205|"));
		}

		public override DbTypeTranslation TranslateRuntimeType(Type type)
		{
			return SqlDbTypeTranslations.TranslateRuntimeType(type);
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

		public override IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText, CommandType cmdType, int cmdTimeout)
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

		public override IDbExecutable DefineExecutableOnConnection(DbConnection connection, string cmdText, CommandType cmdType)
		{
			return new SqlDbExecutable((SqlConnection)connection, cmdText, cmdType);
		}

		public override IDbExecutable DefineExecutableOnConnection(DbConnection connection, string cmdText, CommandType cmdType, int cmdTimeout)
		{
			return new SqlDbExecutable((SqlConnection)connection, cmdText, cmdType, cmdTimeout);
		}

		public override IAsyncResult BeginExecuteNonQuery(DbCommand command, AsyncCallback callback, object stateObject)
		{
			var sql = (SqlCommand) command;
			return sql.BeginExecuteNonQuery(callback, stateObject);
		}
		public override int EndExecuteNonQuery(DbCommand command, IAsyncResult ar)
		{
			var sql = (SqlCommand)command;
			return sql.EndExecuteNonQuery(ar);
		}
		public override IAsyncResult BeginExecuteReader(DbCommand command, AsyncCallback callback, object stateObject)
		{
			var sql = (SqlCommand)command;
			return sql.BeginExecuteReader(callback, stateObject, CommandBehavior.CloseConnection);
		}
		public override DbDataReader EndExecuteReader(DbCommand command, IAsyncResult ar)
		{
			var sql = (SqlCommand)command;
			return sql.EndExecuteReader(ar);
		}
		public override IDataParameterBinder MakeParameterBinder()
		{
			return new SqlParameterBinder();
		}

		public override IDataParameterBinder MakeParameterBinder(DbCommand cmd)
		{
			return new DirectSqlParameterBinder((SqlCommand)cmd);
		}
	}

}
