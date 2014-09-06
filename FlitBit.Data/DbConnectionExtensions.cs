#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using FlitBit.Core;

namespace FlitBit.Data
{
    /// <summary>
    ///     DbConnection extensions.
    /// </summary>
    public static class DbConnectionExtensions
    {
        /// <summary>
        ///     Determines if the catalog\database is accessible on the connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="catalog"></param>
        /// <returns></returns>
        public static bool CatalogExists(this DbConnection connection, string catalog)
        {
            var provider = DbProviderHelpers.GetDbProviderHelperForDbConnection(connection);
            return provider.CatalogExists(connection, catalog);
        }

        /// <summary>
        ///     Creates a command on the specified connection.
        /// </summary>
        /// <param name="connection">the connection</param>
        /// <param name="commandText">the command text</param>
        /// <returns>a newly created command</returns>
        public static DbCommand CreateCommand(this DbConnection connection, string commandText)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(commandText != null);
            Contract.Requires<ArgumentException>(commandText.Length > 0);
            Contract.Ensures(Contract.Result<DbCommand>() != null);
            var res = connection.CreateCommand();
            res.CommandText = commandText;
            return res;
        }

        /// <summary>
        ///     Creates a command on the specified connection.
        /// </summary>
        /// <param name="connection">the connection</param>
        /// <param name="commandText">the command text</param>
        /// <param name="cmdType">the command's type</param>
        /// <returns>a newly created command</returns>
        public static DbCommand CreateCommand(this DbConnection connection, string commandText, CommandType cmdType)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(commandText != null);
            Contract.Requires<ArgumentException>(commandText.Length > 0);
            Contract.Ensures(Contract.Result<DbCommand>() != null);
            var res = connection.CreateCommand();
            res.CommandText = commandText;
            res.CommandType = cmdType;
            return res;
        }

        /// <summary>
        ///     Creates a command on the specified connection.
        /// </summary>
        /// <param name="connection">the connection</param>
        /// <param name="commandText">the command text</param>
        /// <param name="cmdType">the command's type</param>
        /// <param name="cmdTimeout">the command's timeout</param>
        /// <returns>a newly created command</returns>
        public static DbCommand CreateCommand(this DbConnection connection, string commandText, CommandType cmdType,
            int cmdTimeout)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(commandText != null);
            Contract.Requires<ArgumentException>(commandText.Length > 0);
            Contract.Ensures(Contract.Result<DbCommand>() != null);
            var res = connection.CreateCommand();
            res.CommandText = commandText;
            res.CommandType = cmdType;
            res.CommandTimeout = cmdTimeout;
            return res;
        }

        /// <summary>
        ///     Ensures the connection is open.
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
        ///     Executes the given command on the connection.
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
                Contract.Assert(!String.IsNullOrEmpty(cmd.CommandText));

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
        ///     Executes the given command on the connection.
        /// </summary>
        /// <param name="connection">DbConnection upon which the command will be executed.</param>
        /// <param name="command">Text of the command to execute</param>
        /// <returns>An enumerable over the DbDataReader rows.</returns>
        public static IEnumerable<IDataRecord> ImmediateExecuteEnumerable(this DbConnection connection, string command)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentException>(command.Length > 0);
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
        ///     Executes the given command on the connection.
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
            Contract.Requires<ArgumentException>(command.Length > 0);
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
        ///     Executes the given command on the connection.
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
            Contract.Requires<ArgumentException>(command.Length > 0);
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
        ///     Executes the specified command on the specified connection and transforms each result using the specified
        ///     transform.
        /// </summary>
        /// <param name="connection">a connection</param>
        /// <param name="command">The command's text.</param>
        /// <param name="transform">the transform, called for each row of results</param>
        /// <typeparam name="T">result type T</typeparam>
        /// <returns>an unenumerated enumerable of result type T</returns>
        public static IEnumerable<T> ImmediateExecuteEnumerable<T>(this DbConnection connection, string command,
            Func<IDataRecord, T> transform)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentException>(command.Length > 0);
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

        static DbProviderHelper EnsuredDbProviderHelper(DbConnection connection)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Ensures(Contract.Result<DbProviderHelper>() != null);

            var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(connection);
            if (helper == null)
            {
                throw new InvalidOperationException(
                    String.Concat("Unable to get a DbProviderHelper for connections of type: ",
                        connection.GetType().GetReadableSimpleName())
                    );
            }
            return helper;
        }

        /// <summary>
        ///     Executes the specified command on the specified connection and transforms each result using the specified
        ///     transform. Optionally binding parameters with the specified binder.
        /// </summary>
        /// <param name="connection">a connection</param>
        /// <param name="command">The command's text.</param>
        /// <param name="binder">an optional parameter binder</param>
        /// <param name="transform">the transform, called for each row of results</param>
        /// <typeparam name="T">result type T</typeparam>
        /// <returns>an unenumerated enumerable of result type T</returns>
        public static IEnumerable<T> ImmediateExecuteEnumerable<T>(this DbConnection connection, string command,
            Action<IDataParameterBinder> binder,
            Func<IDataRecord, T> transform)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentException>(command.Length > 0);
            Contract.Requires<ArgumentNullException>(transform != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            var helper = EnsuredDbProviderHelper(connection);
            return helper.ExecuteEnumerable(connection, command, binder, transform);
        }
        

        /// <summary>
        ///     Executes the given command on the connection.
        /// </summary>
        /// <param name="connection">DbConnection upon which the command will be executed.</param>
        /// <param name="command">Text of the command to execute</param>
        /// <param name="cmdType">The type of the command being executed: { Text | StoredProcedure | TableDirect }.</param>
        /// <param name="transform">transforms a record into type T</param>
        /// <returns>An enumerable over the DbDataReader rows.</returns>
        public static IEnumerable<T> ImmediateExecuteEnumerable<T>(this DbConnection connection, string command,
            CommandType cmdType, Func<IDataRecord, T> transform)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentException>(command.Length > 0);
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
        ///     Executes the given command on the connection.
        /// </summary>
        /// <param name="connection">DbConnection upon which the command will be executed.</param>
        /// <param name="command">Text of the command to execute</param>
        /// <param name="cmdType">The type of the command being executed: { Text | StoredProcedure | TableDirect }.</param>
        /// <param name="cmdTimeout">the command's timeout.</param>
        /// <param name="transform">transforms a record into type T</param>
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
        ///     Executes the specified command on the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="connection">DbConnection upon which the command will be executed.</param>
        /// <param name="preCommand">an action that prepares the command for execution.</param>
        /// <param name="postCommand">an optional callback invoked after the command completes.</param>
        /// <returns>the number of rows affected.</returns>
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
                Contract.Assert(!String.IsNullOrEmpty(cmd.CommandText));

                var res = cmd.ExecuteNonQuery();
                if (postCommand != null)
                {
                    postCommand(cmd, res);
                }
                return res;
            }
        }

        /// <summary>
        ///     Executes the specified command on the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="connection">DbConnection upon which the command will be executed.</param>
        /// <param name="preCommand">an action that prepares the command for execution.</param>
        /// <param name="binder">an optional callback that will bind the command's parameters</param>
        /// <param name="postCommand">an optional callback invoked after the command completes.</param>
        /// <returns>the number of rows affected.</returns>
        public static int ImmediateExecuteNonQuery(this DbConnection connection, 
            Action<DbCommand> preCommand,
            Action<IDataParameterBinder> binder,
            Action<DbCommand, IDataParameterBinder, int> postCommand)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(preCommand != null);

            var helper = EnsuredDbProviderHelper(connection);
            return helper.ExecuteNonQuery(connection, preCommand, binder, postCommand);
        }

        /// <summary>
        /// Executes the specified command and returns the number of rows affected.
        /// </summary>
        /// <param name="connection">the connection upon which the command will be executed</param>
        /// <param name="command">the command</param>
        /// <returns>the number of rows affected</returns>
        public static int ImmediateExecuteNonQuery(this DbConnection connection, string command)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentException>(command.Length > 0);

            return ImmediateExecuteNonQuery(connection,
                cmd => { cmd.CommandText = command; }, null);
        }

        /// <summary>
        /// Executes the specified command and returns the number of rows affected.
        /// </summary>
        /// <param name="connection">the connection upon which the command will be executed</param>
        /// <param name="command">the command</param>
        /// <param name="binder">a callback method that binds the command's parameters</param>
        /// <param name="postCommand">an optional callback invoked after the command completes.</param>
        /// <returns>the number of rows affected</returns>
        public static int ImmediateExecuteNonQuery(this DbConnection connection, string command, 
            Action<IDataParameterBinder> binder,
            Action<DbCommand, IDataParameterBinder, int> postCommand)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentException>(command.Length > 0);

            return ImmediateExecuteNonQuery(connection,
                cmd => { cmd.CommandText = command; }, binder, postCommand);
        }

        /// <summary>
        /// Executes the specified command and returns the number of rows affected.
        /// </summary>
        /// <param name="connection">the connection upon which the command will be executed</param>
        /// <param name="command">the command</param>
        /// <param name="binder">a callback method that binds the command's parameters</param>
        /// <returns>the number of rows affected</returns>
        public static int ImmediateExecuteNonQuery(this DbConnection connection, string command,
            Action<IDataParameterBinder> binder)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentException>(command.Length > 0);

            return ImmediateExecuteNonQuery(connection,
                cmd => { cmd.CommandText = command; }, binder, null);
        }


        public static int ImmediateExecuteNonQuery(this DbConnection connection, string command, CommandType cmdType)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentException>(command.Length > 0);

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
            Contract.Requires<ArgumentException>(command.Length > 0);

            return ImmediateExecuteNonQuery(connection,
                cmd =>
                {
                    cmd.CommandText = command;
                    cmd.CommandType = cmdType;
                    cmd.CommandTimeout = cmdTimeout;
                }, null);
        }

        /// <summary>
        /// Executes the specified command for each of the specified data items.
        /// </summary>
        /// <param name="connection">the connection upon which the command will be executed</param>
        /// <param name="data">the data items used in the batch</param>
        /// <param name="setup">callback method that called to setup the command</param>
        /// <param name="binder">callback method that binds the command's parameters for each item of data</param>
        /// <param name="observer">an optional callback method for observing the result for each item of data</param>
        /// <returns>an enumerable over the items' results</returns>
        public static IEnumerable<int> ImmediateExecuteNonQueryBatch<TData>(this DbConnection connection, 
            IEnumerable<TData> data,
            Action<DbCommand, IDataParameterBinder> setup,
            Action<DbCommand, IDataParameterBinder, TData> binder,
            Action<DbCommand, IDataParameterBinder, TData, int> observer)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(data != null);
            Contract.Requires<ArgumentNullException>(setup != null);
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Ensures(Contract.Result<IEnumerable<int>>() != null);

            var helper = EnsuredDbProviderHelper(connection);
            return helper.ImmediateExecuteNonQueryBatch(
                connection, 
                data, 
                setup, 
                binder, 
                observer
                );
        }


        /// <summary>
        ///     Determines if a schema exists.
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
        ///     Transforms all records in a reader result to
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="readers"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IEnumerable<T> TransformAll<T>(this IEnumerable<IDataRecord> readers,
            Func<IDataRecord, T> transform)
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