#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace FlitBit.Data
{
	public static class DbConnectionExtensions
	{
		/// <summary>
		///   Determines if the catalog\database is accessible on the connection.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="catalog"></param>
		/// <returns></returns>
		public static bool CatalogExists(this DbConnection connection, string catalog)
		{
			var provider = DbProviderHelpers.GetDbProviderHelperForDbConnection(connection);
			return provider.CatalogExists(connection, catalog);
		}

		public static DbCommand CreateCommand(this DbConnection connection, string command)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);
			Contract.Ensures(Contract.Result<DbCommand>() != null);
			var res = connection.CreateCommand();
			res.CommandText = command;
			return res;
		}

		public static DbCommand CreateCommand(this DbConnection connection, string command, CommandType cmdType)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);
			Contract.Ensures(Contract.Result<DbCommand>() != null);
			var res = connection.CreateCommand();
			res.CommandText = command;
			res.CommandType = cmdType;
			return res;
		}

		public static DbCommand CreateCommand(this DbConnection connection, string command, CommandType cmdType,
			int cmdTimeout)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);
			Contract.Ensures(Contract.Result<DbCommand>() != null);
			var res = connection.CreateCommand();
			res.CommandText = command;
			res.CommandType = cmdType;
			res.CommandTimeout = cmdTimeout;
			return res;
		}

		/// <summary>
		///   Ensures the connection is open.
		/// </summary>
		/// <param name="connection">the connection</param>
		/// <returns>the connection</returns>
		public static TConnection EnsureConnectionIsOpen<TConnection>(this TConnection connection)
			where TConnection : DbConnection
		{
			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}
			return connection;
		}

		/// <summary>
		///   Executes the given command on the connection.
		/// </summary>
		/// <param name="connection">DbConnection upon which the command will be executed.</param>
		/// <param name="prepareCommand">an action that prepares the command for execution.</param>
		/// <returns>An enumerable over the data reader's rows.</returns>
		public static IEnumerable<IDataRecord> ImmediateExecuteEnumerable(this DbConnection connection,
			Action<DbCommand> prepareCommand)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
			Contract.Requires<ArgumentNullException>(prepareCommand != null);
			Contract.Ensures(Contract.Result<IEnumerable<IDataRecord>>() != null);

			using (var cmd = connection.CreateCommand())
			{
				prepareCommand(cmd);
				// Ensure the callback established the command's text...
				Contract.Assert(cmd.CommandText != null && cmd.CommandText.Length > 0);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return reader;
					}
				}
			}
		}

		/// <summary>
		///   Executes the given command on the connection.
		/// </summary>
		/// <param name="connection">DbConnection upon which the command will be executed.</param>
		/// <param name="command">Text of the command to execute</param>
		/// <returns>An enumerable over the DbDataReader rows.</returns>
		public static IEnumerable<IDataRecord> ImmediateExecuteEnumerable(this DbConnection connection, string command)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires(command.Length > 0);
			Contract.Ensures(Contract.Result<IEnumerable<IDataRecord>>() != null);

			using (var cmd = connection.CreateCommand(command))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return reader;
					}
				}
			}
		}

		/// <summary>
		///   Executes the given command on the connection.
		/// </summary>
		/// <param name="connection">DbConnection upon which the command will be executed.</param>
		/// <param name="command">Text of the command to execute</param>
		/// <param name="cmdType">The type of the command being executed: { Text | StoredProcedure | TableDirect }.</param>
		/// <returns>An enumerable over the DbDataReader rows.</returns>
		public static IEnumerable<IDataRecord> ImmediateExecuteEnumerable(this DbConnection connection, string command,
			CommandType cmdType)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires(command.Length > 0);
			Contract.Ensures(Contract.Result<IEnumerable<IDataRecord>>() != null);

			using (var cmd = connection.CreateCommand(command, cmdType))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return reader;
					}
				}
			}
		}

		/// <summary>
		///   Executes the given command on the connection.
		/// </summary>
		/// <param name="connection">DbConnection upon which the command will be executed.</param>
		/// <param name="command">Text of the command to execute</param>
		/// <param name="cmdType">The type of the command being executed: { Text | StoredProcedure | TableDirect }.</param>
		/// <param name="cmdTimeout">the command's timeout.</param>
		/// <returns>An enumerable over the DbDataReader rows.</returns>
		public static IEnumerable<IDataRecord> ImmediateExecuteEnumerable(this DbConnection connection, string command,
			CommandType cmdType, int cmdTimeout)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires(command.Length > 0);
			Contract.Ensures(Contract.Result<IEnumerable<IDataRecord>>() != null);

			using (var cmd = connection.CreateCommand(command, cmdType, cmdTimeout))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return reader;
					}
				}
			}
		}

		/// <summary>
		///   Executes the given command on the connection.
		/// </summary>
		/// <param name="connection">DbConnection upon which the command will be executed.</param>
		/// <param name="command">Text of the command to execute</param>
		/// <returns>An enumerable over the transformed rows.</returns>
		public static IEnumerable<T> ImmediateExecuteEnumerable<T>(this DbConnection connection, string command,
			Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires(command.Length > 0);
			Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

			using (var cmd = connection.CreateCommand(command))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return transform(reader);
					}
				}
			}
		}

		/// <summary>
		///   Executes the given command on the connection.
		/// </summary>
		/// <param name="connection">DbConnection upon which the command will be executed.</param>
		/// <param name="command">Text of the command to execute</param>
		/// <param name="cmdType">The type of the command being executed: { Text | StoredProcedure | TableDirect }.</param>
		/// <returns>An enumerable over the DbDataReader rows.</returns>
		public static IEnumerable<T> ImmediateExecuteEnumerable<T>(this DbConnection connection, string command,
			CommandType cmdType, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires(command.Length > 0);
			Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

			using (var cmd = connection.CreateCommand(command, cmdType))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return transform(reader);
					}
				}
			}
		}

		/// <summary>
		///   Executes the given command on the connection.
		/// </summary>
		/// <param name="connection">DbConnection upon which the command will be executed.</param>
		/// <param name="command">Text of the command to execute</param>
		/// <param name="cmdType">The type of the command being executed: { Text | StoredProcedure | TableDirect }.</param>
		/// <param name="cmdTimeout">the command's timeout.</param>
		/// <returns>An enumerable over the DbDataReader rows.</returns>
		public static IEnumerable<T> ImmediateExecuteEnumerable<T>(this DbConnection connection, string command,
			CommandType cmdType, int cmdTimeout, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentException>(command.Length > 0);
			Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

			using (var cmd = connection.CreateCommand(command, cmdType, cmdTimeout))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return transform(reader);
					}
				}
			}
		}

		/// <summary>
		///   Executes the given command on the connection and returns the number of rows affected.
		/// </summary>
		/// <param name="connection">DbConnection upon which the command will be executed.</param>
		/// <param name="preCommand">an action that prepares the command for execution.</param>
		/// <param name="postCommand">an optional callback invoked after the command completes.</param>
		/// <returns>An enumerable over the data reader's rows.</returns>
		public static int ImmediateExecuteNonQuery(this DbConnection connection, Action<DbCommand> preCommand,
			Action<DbCommand, int> postCommand)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
			Contract.Requires<ArgumentNullException>(preCommand != null);

			using (var cmd = connection.CreateCommand())
			{
				preCommand(cmd);
				// Ensure the callback established the command's text...
				Contract.Assert(cmd.CommandText != null && cmd.CommandText.Length > 0);

				var res = cmd.ExecuteNonQuery();
				if (postCommand != null)
				{
					postCommand(cmd, res);
				}
				return res;
			}
		}

		public static int ImmediateExecuteNonQuery(this DbConnection connection, string command)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires(command.Length > 0);

			return ImmediateExecuteNonQuery(connection,
																			cmd => { cmd.CommandText = command; }, null);
		}

		public static int ImmediateExecuteNonQuery(this DbConnection connection, string command, CommandType cmdType)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires(command.Length > 0);

			return ImmediateExecuteNonQuery(connection,
																			cmd =>
																			{
																				cmd.CommandText = command;
																				cmd.CommandType = cmdType;
																			}, null);
		}

		public static int ImmediateExecuteNonQuery(this DbConnection connection, string command, CommandType cmdType,
			int cmdTimeout)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires(command.Length > 0);

			return ImmediateExecuteNonQuery(connection,
																			cmd =>
																			{
																				cmd.CommandText = command;
																				cmd.CommandType = cmdType;
																				cmd.CommandTimeout = cmdTimeout;
																			}, null);
		}

		/// <summary>
		///   Determines if a schema exists.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="catalog"></param>
		/// <param name="schema"></param>
		/// <returns></returns>
		public static bool SchemaExists(this DbConnection connection, string catalog, string schema)
		{
			var provider = DbProviderHelpers.GetDbProviderHelperForDbConnection(connection);
			return provider.SchemaExists(connection, catalog, schema);
		}

		/// <summary>
		///   Transforms all records in a reader result to
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
	}
}