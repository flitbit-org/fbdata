#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Core.Properties;

namespace FlitBit.Data
{
	public static class DbExtensions
	{
		struct ProviderRecord
		{
			public string ConnectionName;
			public string ConnectionString;
			public string ProviderName;
			public DbProviderFactory Provider;
		}

		static ConcurrentDictionary<string, ProviderRecord> __providers = new ConcurrentDictionary<string, ProviderRecord>();

		/// <summary>
		/// Clears cached providers.
		/// </summary>
		public static void ResetProviders()
		{
			__providers = new ConcurrentDictionary<string, ProviderRecord>();      
		}

		public static string GetProviderName(string connectionName)
		{
			Contract.Requires<ArgumentNullException>(connectionName != null);
			Contract.Requires(connectionName.Length > 0);

			return AccessProvider(connectionName).ProviderName;
		}

		public static DbProviderFactory GetProviderByConnectionName(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires(name.Length > 0);
			
			return AccessProvider(name).Provider;
		}

		private static ProviderRecord AccessProvider(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires(name.Length > 0);

			return __providers.GetOrAdd(name, (n) =>
				{
					ConnectionStringSettings css = ConfigurationManager.ConnectionStrings[name];
					if (css == null) 
						throw new ConfigurationErrorsException(String.Concat("Connection string not found: ", name));
					var r = new ProviderRecord();
					r.ConnectionName = name;
					r.ConnectionString = css.ConnectionString;
					r.ProviderName = css.ProviderName;
					r.Provider = DbProviderFactories.GetFactory(css.ProviderName);
					return r;
				});
		}

		/// <summary>
		/// Creates a new IDbConnection initialized with the specified connection name. The connection name 
		/// must be present in the &lt;connectionStrings&gt; section of the application's config file
		/// </summary>
		/// <param name="name">A connection string name declared in the application's config file.</param>
		/// <returns>The IDbConnection initialized with the specified connection string.</returns>
		/// <exception cref="System.ContractException">thrown if the connection name is not specified.</exception>
		/// <exception cref="System.Configuration.ConfigurationErrorsException">thrown if the connection name is not found in the application's config file.</exception>
		public static IDbConnection CreateConnection(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires(name.Length > 0);
			
			var r = AccessProvider(name);
			var cn = r.Provider.CreateConnection();
			cn.ConnectionString = r.ConnectionString;
			return cn;
		}

		/// <summary>
		/// Creates and opens a new IDbConnection initialized with the specified connection name. 
		/// The connection name must be present in the &lt;connectionStrings&gt; section of the 
		/// application's config file
		/// </summary>
		/// <param name="name">A connection string name declared in the application's config file.</param>
		/// <returns>An open connection to the database specified in the connection string.</returns>
		/// <exception cref="System.ContractException">thrown if the connection name is not specified.</exception>
		/// <exception cref="System.Configuration.ConfigurationErrorsException">thrown if the connection name is not found in the application's config file.</exception>
		public static IDbConnection CreateAndOpenConnection(string name)
		{
			var result = DbExtensions.CreateConnection(name);
			try
			{
				result.Open();
			}
			catch
			{
				result.Dispose();
				throw;
			}
			return result;
		}

		/// <summary>
		/// Ensures the connection is open.
		/// </summary>
		/// <param name="connection">the connection</param>
		/// <returns>the connection</returns>
		public static IDbConnection EnsureConnectionIsOpen(this IDbConnection connection)
		{
			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}
			return connection;
		}

		/// <summary>
		/// Determines if the catalog\database is accessible on the connection.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="catalog"></param>
		/// <returns></returns>
		public static bool CatalogExists(this IDbConnection connection, string catalog)
		{
			var provider = DbProviderHelpers.GetDbProviderHelperForDbConnection(connection);
			return provider.CatalogExists(connection, catalog);
		}

		/// <summary>
		/// Determines if a schema exists.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="catalog"></param>
		/// <param name="schema"></param>
		/// <returns></returns>
		public static bool SchemaExists(this IDbConnection connection, string catalog, string schema)
		{
			var provider = DbProviderHelpers.GetDbProviderHelperForDbConnection(connection);
			return provider.SchemaExists(connection, catalog, schema);
		}

		/// <summary>
		/// Creates a new IDbCommand object initialized with the command string given.
		/// </summary>
		/// <param name="connection">IDbConnection upon which the command will be created.</param>
		/// <param name="command">Text of the command.</param>
		/// <returns>a new IDbCommand</returns>
		public static IDbCommand CreateCommand(this IDbConnection connection, string command)
		{
			return CreateCommand(connection, command, CommandType.Text);
		}

		/// <summary>
		/// Creates a new IDbCommand object initialized with the command string given.
		/// </summary>
		/// <param name="connection">IDbConnection upon which the command will be created.</param>
		/// <param name="command">Text of the command.</param>
		/// <param name="type">The type of the command to create: { Text | StoredProcedure | TableDirect }.</param>
		/// <returns>a new IDbCommand</returns>
		[SuppressMessage("Microsoft.Security", "CA2100", Justification = "By design; although callers should be very careful.")]
		public static IDbCommand CreateCommand(this IDbConnection connection, string command, CommandType type)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(command != null);

			IDbCommand cmd = connection.CreateCommand();
			cmd.CommandText = command;
			cmd.CommandType = type;
			return cmd;
		}

		/// <summary>
		/// Creates and initializes an IDbCommand instance.
		/// </summary>
		/// <param name="cn">Connection used to create the command.</param>
		/// <param name="cmdText">The new command's CommandText.</param>
		/// <param name="cmdType">The new command's CommandType.</param>
		/// <param name="cmdTimeout">The new command's CommandTimeout.</param>
		/// <returns>a new IDbCommand</returns>
		public static IDbCommand CreateCommand(this IDbConnection cn, string cmdText, CommandType cmdType, int cmdTimeout)
		{
			IDbCommand cmd = cn.CreateCommand();
			cmd.CommandText = cmdText;
			cmd.CommandTimeout = cmdTimeout;
			cmd.CommandType = cmdType;
			return cmd;
		}																		

		/// <summary>
		/// Executes the given command on the connection.
		/// </summary>
		/// <param name="connection">IDbConnection upon which the command will be executed.</param>
		/// <param name="command">Text of the command to execute</param>
		/// <returns>An enumerable over the IDataReader rows.</returns>
		public static IEnumerable<IDataRecord> ExecuteReader(this IDbConnection connection, string command)
		{
			return ExecuteReader(connection, command, CommandType.Text);
		}

		/// <summary>
		/// Executes the given command on the connection.
		/// </summary>
		/// <param name="connection">IDbConnection upon which the command will be executed.</param>
		/// <param name="command">Text of the command to execute</param>
		/// <param name="type">The type of the command being executed: { Text | StoredProcedure | TableDirect }.</param>
		/// <returns>An enumerable over the IDataReader rows.</returns>
		public static IEnumerable<IDataRecord> ExecuteReader(this IDbConnection connection, string command, CommandType type)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires(command.Length > 0);

			if (DbTraceEvents.ShouldTrace(TraceEventType.Verbose))
			{
				DbTraceEvents.OnTraceEvent(connection, TraceEventType.Verbose, 
					String.Concat("ExecuteReader: ", command));
			}						 			

			using (IDbCommand cmd = connection.CreateCommand(command, type))
			{
				using (IDataReader reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						yield return reader;
					}
				}
			}
		}

		/// <summary>
		/// Executes the given command on the connection.
		/// </summary>
		/// <param name="connection">IDbConnection upon which the command will be executed.</param>
		/// <param name="command">Text of the command to execute</param>
		/// <param name="type">The type of the command being executed: { Text | StoredProcedure | TableDirect }.</param>
		/// <param name="cmdTimeout">the command's timeout.</param>
		/// <returns>An enumerable over the IDataReader rows.</returns>
		public static IEnumerable<IDataRecord> ExecuteReader(this IDbConnection connection, string command, CommandType type, int cmdTimeout )
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires(command.Length > 0);

			if (DbTraceEvents.ShouldTrace(TraceEventType.Verbose))
			{
				DbTraceEvents.OnTraceEvent(connection, TraceEventType.Verbose,
					String.Concat("ExecuteReader: ", command));
			}

			using (IDbCommand cmd = connection.CreateCommand(command, type, cmdTimeout))
			{
				using (IDataReader reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						yield return reader;
					}
				}
			}
		}

		/// <summary>
		/// Executes the command and returns an enumerable over the resulting records.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public static IEnumerable<IDataRecord> ExecuteEnumerable(this IDbCommand command)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<InvalidOperationException>(command.Connection != null);
			Contract.Requires<InvalidOperationException>(command.CommandText.Length > 0);
			Contract.Ensures(Contract.Result<IEnumerable<IDataRecord>>() != null);

			if (DbTraceEvents.ShouldTrace(TraceEventType.Verbose))
			{
				DbTraceEvents.OnTraceEvent(command, TraceEventType.Verbose,
					String.Concat("ExecuteEnumerable: ", command));
			}

			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					yield return reader;
				}
			}
		}

		/// <summary>
		/// Gets a single data record from a reader result.
		/// </summary>
		/// <param name="readers">a reader result</param>
		/// <returns>a single record from the reader result</returns>
		/// <exception cref="System.InvalidOperationException">thrown if the reader result contains more than one record.</exception>
		public static IDataRecord SingleRecord(this IEnumerable<IDataRecord> readers)
		{
			Contract.Requires<ArgumentNullException>(readers != null);
			return readers.Single();
		}

		/// <summary>
		/// Determines if a single result row exists with a non-null
		/// value in the specified column.
		/// </summary>
		/// <param name="readers">a reader result</param>
		/// <param name="column">index of the column to check for a value</param>
		/// <returns><em>true</em> if a single result row exists with a non-null
		/// value in the specified column; otherwise <em>false</em></returns>
		/// <exception cref="System.IndexOutOfRangeException">thrown if column given is out of range.</exception>
		public static bool SingleExists(this IEnumerable<IDataRecord> readers, int column)
		{
			Contract.Requires<ArgumentNullException>(readers != null);

			var single = readers.SingleOrDefault();
			return single != null && single.IsDBNull(column) == false;
		}

		/// <summary>
		/// Determines if a single result row exists with a non-null
		/// value in the specified column.
		/// </summary>
		/// <param name="readers">a reader result</param>
		/// <param name="column">index of the column to check for a value</param>
		/// <returns><em>true</em> if a single result row exists with a non-null
		/// value in the specified column; otherwise <em>false</em></returns>
		/// <exception cref="System.IndexOutOfRangeException">thrown if column given is not defined.</exception>
		public static bool SingleExists(this IEnumerable<IDataRecord> readers, string column)
		{
			Contract.Requires<ArgumentNullException>(readers != null);
			Contract.Requires<ArgumentNullException>(column != null);
						
			var single = readers.SingleOrDefault();
			return single != null && single.IsDBNull(single.GetOrdinal(column)) == false;
		}

		/// <summary>
		/// Determines if the first result row exists with a non-null
		/// value in the specified column.
		/// </summary>
		/// <param name="readers">a reader result</param>
		/// <param name="column">index of the column to check for a value</param>
		/// <returns><em>true</em> if the first result row exists with a non-null
		/// value in the specified column; otherwise <em>false</em></returns>
		/// <exception cref="System.IndexOutOfRangeException">thrown if column given is out of range.</exception>
		public static bool FirstExists(this IEnumerable<IDataRecord> readers, int column)
		{
			Contract.Requires<ArgumentNullException>(readers != null);

			var first = readers.FirstOrDefault();
			return first != null && first.IsDBNull(column) == false;
		}

		/// <summary>
		/// Determines if the first result row exists with a non-null
		/// value in the specified column.
		/// </summary>
		/// <param name="readers">a reader result</param>
		/// <param name="column">index of the column to check for a value</param>
		/// <returns><em>true</em> if the first result row exists with a non-null
		/// value in the specified column; otherwise <em>false</em></returns>
		/// <exception cref="System.IndexOutOfRangeException">thrown if column given is out of range.</exception>
		public static bool FirstExists(this IEnumerable<IDataRecord> readers, string column)
		{
			Contract.Requires<ArgumentNullException>(readers != null);

			var first = readers.FirstOrDefault();
			return first != null && first.IsDBNull(first.GetOrdinal(column)) == false;
		}
		
		/// <summary>
		/// Transforms all records in a reader result to
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="readers"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static IEnumerable<T> TransformAll<T>(this IEnumerable<IDataRecord> readers, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(readers != null);
			Contract.Requires<ArgumentNullException>(transform != null);

			foreach (var reader in readers)
			{
				yield return transform(reader);
			}
		}
		
		/// <summary>
		/// Transforms a single record in a reader result to type T.
		/// </summary>
		/// <typeparam name="T">target type T</typeparam>
		/// <param name="readers">a reader result</param>
		/// <param name="transform">a transformation method</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">thrown if the reader result contains more than one record.</exception>
		public static T TransformSingle<T>(this IEnumerable<IDataRecord> readers, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(readers != null);
			Contract.Requires<ArgumentNullException>(transform != null);

			var first = readers.Single();
			return transform(first);			
		}

		public static T TransformSingleOrDefault<T>(this IEnumerable<IDataRecord> readers, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(readers != null);
			Contract.Requires<ArgumentNullException>(transform != null);

			var single = readers.SingleOrDefault();
			if (single == null) return default(T);
			return transform(single);
		}

		public static T TransformFirst<T>(this IEnumerable<IDataReader> readers, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(readers != null);
			Contract.Requires<ArgumentNullException>(transform != null);

			var first = readers.First();
			return transform(first);
		}

		public static T TransformFirstOrDefault<T>(this IEnumerable<IDataRecord> readers, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(readers != null);
			Contract.Requires<ArgumentNullException>(transform != null);

			var first = readers.FirstOrDefault();
			if (first == null) return default(T);
			return transform(first);
		}

		public static IDbCommand BindParameters(this IDbCommand command, Action<IDataParameterBinder> binder)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(binder != null);

			binder(DataParameterBinders.GetBinderForDbCommand(command));
			return command;
		}
				
		public static int ImmediateExecuteNonQuery(this IDbConnection connection, string command)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(command != null);

			if (DbTraceEvents.ShouldTrace(TraceEventType.Verbose))
			{
				DbTraceEvents.OnTraceEvent(connection, TraceEventType.Verbose,
					String.Concat("ImmediateExecuteNonQuery: ", command));
			}	

			using (IDbCommand cmd = connection.CreateCommand(command))
			{
				return cmd.ExecuteNonQuery();        
			}
		}

		public static int ImmediateExecuteNonQuery(this IDbConnection connection, string command, CommandType commandType)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(command != null);

			if (DbTraceEvents.ShouldTrace(TraceEventType.Verbose))
			{
				DbTraceEvents.OnTraceEvent(connection, TraceEventType.Verbose,
					String.Concat("ImmediateExecuteNonQuery: ", command));
			}

			using (IDbCommand cmd = connection.CreateCommand(command, commandType))
			{
				return cmd.ExecuteNonQuery();
			}
		}		
	}
}
