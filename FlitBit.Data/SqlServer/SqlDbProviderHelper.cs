#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion


using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using FlitBit.Core.Properties;

namespace FlitBit.Data.SqlServer
{																																					
	public class SqlDbProviderHelper : DbProviderHelper
	{
		IDbExecutable _catalogExists = DbExecutable.FromCommandText(@"SELECT CONVERT(BIT, CASE ISNULL(DB_ID(@catalog),-1) WHEN -1 THEN 0 ELSE 1 END) AS HasCatalog")
			.DefineParameter("@catalog", DbType.String);

		IDbExecutable _schemaExists = DbExecutable.FromCommandText(@"
SELECT name AS SCHEMA_NAME 
FROM sys.schemas 
WHERE name = @schema")
			.DefineParameter("@schema", DbType.String);

		public SqlDbProviderHelper()
		{
			base.Factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
		}

		public override bool CatalogExists(IDbConnection connection, string catalog)
		{
			Contract.Assert(typeof(SqlConnection).IsInstanceOfType(connection), "Invalid IDbConnection for DbProvider");

			var scn = (SqlConnection)connection;
			using (var create = DbContext.SharedOrNewContext())
			{
				return create.Add(_catalogExists.CreateOnConnection(scn))
					.SetParameterValue("@catalog", catalog)
					.ExecuteTransformSingleOrDefault(create, r => r.GetBoolean(r.GetOrdinal("HasCatalog")));
			}
		}

		public override bool SchemaExists(IDbConnection connection, string catalog, string schema)
		{
			Contract.Assert(typeof(SqlConnection).IsInstanceOfType(connection), "Invalid IDbConnection for DbProvider");
			
			var scn = (SqlConnection)connection;
			if (!String.Equals(scn.Database, catalog, StringComparison.OrdinalIgnoreCase))
			{
				scn.ChangeDatabase(catalog);
			}
			using (var create = DbContext.SharedOrNewContext())
			{
				return create.Add(_schemaExists.CreateOnConnection(scn))
					.SetParameterValue("@schema", schema)
					.ExecuteTransformSingleOrDefault(create, r => !r.IsDBNull(r.GetOrdinal("SCHEMA_NAME")));					
			}
		}
						
		public override bool SupportsMultipleActiveResultSets(IDbConnection connection)
		{
			Contract.Assert(typeof(SqlConnection).IsInstanceOfType(connection), "Invalid IDbConnection for DbProvider");

			SqlConnection scn = (SqlConnection)connection;
			SqlConnectionStringBuilder cnsb = new SqlConnectionStringBuilder(scn.ConnectionString);
			return cnsb.MultipleActiveResultSets;
		}

		public override bool SupportsAsynchronousProcessing(IDbConnection connection)
		{
			Contract.Assert(typeof(SqlConnection).IsInstanceOfType(connection), "Invalid IDbConnection for DbProvider");

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
				
		public override string GetServerName(IDbConnection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			return connection.ExecuteReader("SELECT @@SERVERNAME").SingleRecord().GetString(0);
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

		public override BasicDbExecutable DefineCommandOnConnection(string connection)
		{
			return new SqlDbCommand(connection);
		}
		public override BasicDbExecutable DefineCommandOnConnection(string connection, string commandText, CommandType commandType)
		{
			return new SqlDbCommand(connection, commandText, commandType);
		}
		public override BasicDbExecutable MakeCommandOnConnection(string connection, BasicDbExecutable definition)
		{
			return new SqlDbCommand(connection, definition);
		}
		public override BasicDbExecutable MakeCommandOnConnection(IDbConnection connection, BasicDbExecutable definition)
		{
			Contract.Assert(typeof(SqlConnection).IsInstanceOfType(connection), "Invalid IDbConnection for DbProvider; must be SqlConnection");
			return new SqlDbCommand(connection, definition);
		}

		public override DbTypeTranslation TranslateRuntimeType(Type type)
		{
			return SqlDbTypeTranslations.TranslateRuntimeType(type);
		}
	}

}
