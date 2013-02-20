#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion
			 
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace FlitBit.Data
{
	
	public abstract class DbProviderHelper
	{
		public DbProviderFactory Factory { get; protected set; }

		public virtual bool SupportsMultipleActiveResultSets(IDbConnection connection)
		{
			return false;
		}

		public abstract bool SupportsAsync { get; }

		/// <summary>
		/// Determines if a catalog (database) exists on the connection.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="catalog"></param>
		public virtual bool CatalogExists(IDbConnection connection, string catalog)
		{
			throw new NotImplementedException();
		}

		public virtual bool SchemaExists(IDbConnection connection, string catalog, string schema)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires(schema.Length > 0);

			var sql = String.Format("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE CATALOG_NAME = '{0}' AND SCHEMA_NAME = '{1}'", catalog, schema);
			return connection.ExecuteReader(sql).SingleExists("SCHEMA_NAME");
		}
				
		public bool TableExists(IDbConnection connection, string catalog, string schema, string table)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires<ArgumentException>(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires(schema.Length > 0);
			Contract.Requires<ArgumentNullException>(table != null);
			Contract.Requires(table.Length > 0);

			return connection
				.ExecuteReader(FormatTableExistsQuery(catalog, schema, table))
				.SingleExists("TABLE_NAME");
		}

		protected virtual string FormatTableExistsQuery(string catalogName, string schemaName, string tableName)
		{
			return String.Format(@"SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_CATALOG = '{0}'
	AND TABLE_SCHEMA = '{1}'
	AND TABLE_NAME = '{2}'
	AND TABLE_TYPE = 'BASE TABLE'", catalogName, schemaName, tableName);
		}

		public bool ViewExists(IDbConnection connection, string catalog, string schema, string view)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires(schema.Length > 0);
			Contract.Requires<ArgumentNullException>(view != null);
			Contract.Requires(view.Length > 0);

			return connection
				.ExecuteReader(FormatViewExistsQuery(catalog, schema, view))
				.SingleExists("TABLE_NAME");
		}

		protected virtual string FormatViewExistsQuery(string catalogName, string schemaName, string viewName)
		{
			return String.Format(@"SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_CATALOG = '{0}'
	AND TABLE_SCHEMA = '{1}'
	AND TABLE_NAME = '{2}'", catalogName, schemaName, viewName);
		}

		public bool StoredProcedureExists(IDbConnection connection, string catalog, string schema, string storedProcedure)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires(schema.Length > 0);
			Contract.Requires<ArgumentNullException>(storedProcedure != null);
			Contract.Requires(storedProcedure.Length > 0);

			return connection
				.ExecuteReader(FormatProcedureExistsQuery(catalog, schema, storedProcedure))
				.SingleExists("ROUTINE_NAME");
		}

		protected virtual string FormatProcedureExistsQuery(string catalog, string schema, string storedProcedure)
		{
			return String.Format(@"SELECT ROUTINE_NAME
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_CATALOG = '{0}'
	AND ROUTINE_SCHEMA = '{1}'
	AND ROUTINE_NAME = '{2}'
	AND ROUTINE_TYPE = 'PROCEDURE'", catalog, schema, storedProcedure);
		}

		public bool FunctionExists(IDbConnection connection, string catalog, string schema, string fun)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires(schema.Length > 0);
			Contract.Requires<ArgumentNullException>(fun != null);
			Contract.Requires(fun.Length > 0);

			return connection
				.ExecuteReader(FormatFunctionExistsQuery(catalog, schema, fun))
				.SingleExists("ROUTINE_NAME");
		}

		protected virtual string FormatFunctionExistsQuery(string catalog, string schema, string fun)
		{
			return String.Format(@"SELECT ROUTINE_NAME
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_CATALOG = '{0}'
	AND ROUTINE_SCHEMA = '{1}'
	AND ROUTINE_NAME = '{2}'
	AND ROUTINE_TYPE = 'FUNCTION'", catalog, schema, fun);
		}

		public abstract string GetServerName(IDbConnection connection);

		public void CreateSchema(IDbConnection connection, string catalog, string schema)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires(schema.Length > 0);

			connection.ImmediateExecuteNonQuery(FormatCreateSchemaCommandText(catalog, schema));
		}

		protected abstract string FormatCreateSchemaCommandText(string catalogName, string schemaName);

		public abstract string FormatParameterName(string rawParameterName);

		public virtual string QuoteObjectName(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires(name.Length > 0);

			return (name[0] != '[') ? String.Concat('[', name, ']') : name;
		}

		public virtual bool CanRetryTransaction(Exception ex)
		{
			return false;
		}

		public virtual BasicDbExecutable DefineCommandOnConnection(string connection)
		{
			return new BasicDbExecutable(connection);
		}
		public virtual BasicDbExecutable DefineCommandOnConnection(string connection, string commandText, CommandType commandType)
		{
			return new BasicDbExecutable(connection, commandText, commandType);
		}
		public virtual BasicDbExecutable MakeCommandOnConnection(string connection, BasicDbExecutable definition)
		{
			return new BasicDbExecutable(connection, definition);
		}
		public virtual BasicDbExecutable MakeCommandOnConnection(IDbConnection connection, BasicDbExecutable definition)
		{
			return new BasicDbExecutable(connection, definition);
		}

		public abstract DbTypeTranslation TranslateRuntimeType(Type type);
	}	
}
