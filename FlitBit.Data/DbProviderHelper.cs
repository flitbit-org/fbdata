#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;

namespace FlitBit.Data
{		
	public abstract class DbProviderHelper
	{
		public DbProviderFactory Factory { get; protected set; }

		public virtual bool SupportsMultipleActiveResultSets(DbConnection connection)
		{
			return false;
		}

		public virtual bool SupportsAsynchronousProcessing(DbConnection connection)
		{
			return false;
		}																	
		
		/// <summary>
		/// Determines if a catalog (database) exists on the connection.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="catalog"></param>
		public virtual bool CatalogExists(DbConnection connection, string catalog)
		{
			throw new NotImplementedException();
		}

		public virtual bool SchemaExists(DbConnection connection, string catalog, string schema)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires(schema.Length > 0);

			var sql = String.Format("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE CATALOG_NAME = '{0}' AND SCHEMA_NAME = '{1}'", catalog, schema);
			return connection.ImmediateExecuteEnumerable(sql).Any();
		}
				
		public bool TableExists(DbConnection connection, string catalog, string schema, string table)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires<ArgumentException>(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires(schema.Length > 0);
			Contract.Requires<ArgumentNullException>(table != null);
			Contract.Requires(table.Length > 0);

			return connection
				.ImmediateExecuteEnumerable(FormatTableExistsQuery(catalog, schema, table))
				.Any();
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

		public bool ViewExists(DbConnection connection, string catalog, string schema, string view)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires(schema.Length > 0);
			Contract.Requires<ArgumentNullException>(view != null);
			Contract.Requires(view.Length > 0);

			return connection
				.ImmediateExecuteEnumerable(FormatViewExistsQuery(catalog, schema, view))
				.Any();
		}

		protected virtual string FormatViewExistsQuery(string catalogName, string schemaName, string viewName)
		{
			return String.Format(@"SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_CATALOG = '{0}'
	AND TABLE_SCHEMA = '{1}'
	AND TABLE_NAME = '{2}'", catalogName, schemaName, viewName);
		}

		public bool StoredProcedureExists(DbConnection connection, string catalog, string schema, string storedProcedure)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires(schema.Length > 0);
			Contract.Requires<ArgumentNullException>(storedProcedure != null);
			Contract.Requires(storedProcedure.Length > 0);

			return connection
				.ImmediateExecuteEnumerable(FormatProcedureExistsQuery(catalog, schema, storedProcedure))
				.Any();
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

		public bool FunctionExists(DbConnection connection, string catalog, string schema, string fun)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(catalog != null);
			Contract.Requires(catalog.Length > 0);
			Contract.Requires<ArgumentNullException>(schema != null);
			Contract.Requires(schema.Length > 0);
			Contract.Requires<ArgumentNullException>(fun != null);
			Contract.Requires(fun.Length > 0);

			return connection
				.ImmediateExecuteEnumerable(FormatFunctionExistsQuery(catalog, schema, fun))
				.Any();
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

		public abstract string GetServerName(DbConnection connection);

		public void CreateSchema(DbConnection connection, string catalog, string schema)
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
			Contract.Requires<ArgumentException>(name.Length > 0);
			Contract.Ensures(Contract.Result<string>() != null);

			return (name[0] != '[') ? String.Concat('[', name, ']') : name;
		}

		public virtual bool CanRetryTransaction(Exception ex)
		{
			return false;
		}
				
		public virtual IAsyncResult BeginExecuteNonQuery(DbCommand command, AsyncCallback callback, Object stateObject) 
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Ensures(Contract.Result<IAsyncResult>() != null);
		
			throw new NotImplementedException();		
		}

		public virtual IAsyncResult BeginExecuteReader(DbCommand command, AsyncCallback callback, Object stateObject)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Ensures(Contract.Result<IAsyncResult>() != null);

			throw new NotImplementedException();
		}

		public virtual int EndExecuteNonQuery(DbCommand command, IAsyncResult ar)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(ar != null);

			throw new NotImplementedException();
		}
		public virtual DbDataReader EndExecuteReader(DbCommand command, IAsyncResult ar)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(ar != null);
			Contract.Ensures(Contract.Result<DbDataReader>() != null);

			throw new NotImplementedException();
		}																								

		public abstract DbTypeTranslation TranslateRuntimeType(Type type);

		public abstract IDbExecutable DefineExecutableOnConnection(string connectionName);

		public abstract IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText);

		public abstract IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText, CommandType cmdType);

		public abstract IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText, CommandType cmdType, int cmdTimeout);

		public abstract IDbExecutable DefineExecutableOnConnection(string connectionName, IDbExecutable exe);

		public abstract IDbExecutable DefineExecutableOnConnection(DbConnection connection, IDbExecutable exe);

		public abstract IDbExecutable DefineExecutableOnConnection(DbConnection connection, string cmdText);

		public abstract IDbExecutable DefineExecutableOnConnection(DbConnection connection, string cmdText, CommandType cmdType);

		public abstract IDbExecutable DefineExecutableOnConnection(DbConnection connection, string cmdText, CommandType cmdType, int cmdTimeout);

		public abstract IDataParameterBinder MakeParameterBinder();

		public abstract IDataParameterBinder MakeParameterBinder(DbCommand cmd);
	}	
}
