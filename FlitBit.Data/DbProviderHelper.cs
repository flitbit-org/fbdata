#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using FlitBit.Core;
using FlitBit.Data.DataModel;
using FlitBit.Data.DataModel.DbTypeEmitters;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data
{
    /// <summary>
    ///     Utility class that provides additional abstractions to the data
    ///     framework when working with DbProviderFactories and DbConnections.
    /// </summary>
    public abstract class DbProviderHelper
    {
        readonly ConcurrentDictionary<Type, MappedDbTypeEmitter> _emitters =
            new ConcurrentDictionary<Type, MappedDbTypeEmitter>();

        bool _initialized;
        string _commandTextSchemaExists;
        string _commandTextTableExists;
        string _commandTextViewExists;
        string _commandTextStoredProcExists;
        string _commandTextFunctionExists;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="dbconnectionType"></param>
        protected DbProviderHelper(Type dbconnectionType)
        {
            Contract.Requires<ArgumentException>(dbconnectionType == null
                                                 || typeof(DbConnection).IsAssignableFrom(dbconnectionType));
            DbConnectionType = dbconnectionType ?? typeof(DbConnection);
        }

        /// <summary>
        ///     Initializes the helper.
        /// </summary>
        public virtual void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            MapRuntimeType<bool>(new MappedBooleanEmitter());
            MapRuntimeType<bool?>(new MappedNullableBooleanEmitter());
            MapRuntimeType<byte>(new MappedByteEmitter());
            MapRuntimeType<byte[]>(new MappedBinaryEmitter());
            MapRuntimeType<char>(new MappedCharEmitter(DbType.String));
            MapRuntimeType<short>(new MappedInt16Emitter());
            MapRuntimeType<string>(new MappedStringEmitter(DbType.String));
            MapRuntimeType<int>(new MappedInt32Emitter());
            MapRuntimeType<long>(new MappedInt16Emitter());
            MapRuntimeType<sbyte>(new MappedSByteEmitter());
            MapRuntimeType<UInt16>(new MappedUInt16Emitter());
            MapRuntimeType<UInt32>(new MappedUInt32Emitter());
            MapRuntimeType<UInt64>(new MappedUInt64Emitter());
            MapRuntimeType<decimal>(new MappedDecimalEmitter());
            MapRuntimeType<double>(new MappedDoubleEmitter());
            MapRuntimeType<float>(new MappedSingleEmitter());
            MapRuntimeType<DateTime>(new MappedDateTimeEmitter());
            MapRuntimeType<DateTimeOffset>(new MappedDateTimeOffsetEmitter());
            MapRuntimeType<Type>(new MappedTypeToStringEmitter(DbType.String));
            _initialized = true;
        }

        /// <summary>
        ///     Registers a mapping between the runtime type T and the specified MappedDbTypeEmitter.
        /// </summary>
        /// <param name="map">the mapping emitter</param>
        /// <typeparam name="T">the runtime type T</typeparam>
        protected void MapRuntimeType<T>(MappedDbTypeEmitter map)
        {
            var key = typeof(T);
            _emitters.AddOrUpdate(key, map, (k, it) => map);
        }

        /// <summary>
        ///     Gets the helper's DbProviderFactory.
        /// </summary>
        public DbProviderFactory Factory { get; protected set; }

        /// <summary>
        ///     The concrete DbConnection type that the factory and this hepler
        ///     uses and extends.
        /// </summary>
        public Type DbConnectionType { get; private set; }

        /// <summary>
        ///     Defines an executable on the specified connection.
        /// </summary>
        /// <param name="connectionName">the connection's name</param>
        /// <returns></returns>
        public abstract IDbExecutable DefineExecutableOnConnection(string connectionName);

        /// <summary>
        ///     Defines an executable for the specified command text on the specified connection.
        /// </summary>
        /// <param name="connectionName">the connection's name</param>
        /// <param name="cmdText">the executable's command text</param>
        /// <returns></returns>
        public abstract IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText);

        /// <summary>
        ///     Defines an executable for the specified command text on the specified connection.
        /// </summary>
        /// <param name="connectionName">the connection's name</param>
        /// <param name="cmdText">the executable's command text</param>
        /// <param name="cmdType">the command's type</param>
        /// <returns></returns>
        public abstract IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText,
            CommandType cmdType);

        /// <summary>
        ///     Defines an executable for the specified command text on the specified connection.
        /// </summary>
        /// <param name="connectionName">the connection's name</param>
        /// <param name="cmdText">the executable's command text</param>
        /// <param name="cmdType">the command's type</param>
        /// <param name="cmdTimeout">the command's timeout</param>
        /// <returns></returns>
        public abstract IDbExecutable DefineExecutableOnConnection(string connectionName, string cmdText,
            CommandType cmdType,
            int cmdTimeout);

        /// <summary>
        ///     Defines an executable on the specified connection from the specified executable definition.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="definition"></param>
        /// <returns></returns>
        public abstract IDbExecutable DefineExecutableOnConnection(string connectionName, IDbExecutable definition);

        /// <summary>
        ///     Defines an executable on the specified connection from the specified executable definition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="definition"></param>
        /// <returns></returns>
        public abstract IDbExecutable DefineExecutableOnConnection(DbConnection connection, IDbExecutable definition);

        /// <summary>
        ///     Defines an executable for the specified command text on the specified connection.
        /// </summary>
        /// <param name="connection">the connection's name</param>
        /// <param name="cmdText">the executable's command text</param>
        /// <returns></returns>
        public abstract IDbExecutable DefineExecutableOnConnection(DbConnection connection, string cmdText);

        /// <summary>
        ///     Defines an executable for the specified command text on the specified connection.
        /// </summary>
        /// <param name="connection">the connection's name</param>
        /// <param name="cmdText">the executable's command text</param>
        /// <param name="cmdType">the command's type</param>
        /// <returns></returns>
        public abstract IDbExecutable DefineExecutableOnConnection(DbConnection connection, string cmdText,
            CommandType cmdType);

        /// <summary>
        ///     Defines an executable for the specified command text on the specified connection.
        /// </summary>
        /// <param name="connection">the connection's name</param>
        /// <param name="cmdText">the executable's command text</param>
        /// <param name="cmdType">the command's type</param>
        /// <param name="cmdTimeout">the command's timeout</param>
        /// <returns></returns>
        public abstract IDbExecutable DefineExecutableOnConnection(DbConnection connection, string cmdText,
            CommandType cmdType, int cmdTimeout);

        /// <summary>
        ///     Formats a raw parameter name using the underlying database's naming rules for parameters.
        /// </summary>
        /// <param name="rawParameterName"></param>
        /// <returns></returns>
        public abstract string FormatParameterName(string rawParameterName);

        /// <summary>
        ///     For use with data models; gets a data model binder for the specified generic argument TDataModel
        ///     with the specified generic arguement TIdentity.
        /// </summary>
        /// <typeparam name="TDataModel">data model type TDataModel</typeparam>
        /// <typeparam name="TIdentity">identity type TIdentity</typeparam>
        /// <param name="mapping">The data model's mapping.</param>
        /// <returns>the data model's binder implementation</returns>
        public abstract IDataModelBinder<TDataModel, TIdentity> GetModelBinder<TDataModel, TIdentity>(
            IMapping<TDataModel> mapping);

        /// <summary>
        ///     Gets the server name for the specified connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public abstract string GetServerName(DbConnection connection);

        /// <summary>
        ///     Factory method; creates a parameter binder.
        /// </summary>
        /// <returns>a parameter binder</returns>
        public abstract IDataParameterBinder MakeParameterBinder();

        /// <summary>
        ///     Factory method; creates a parameter binder for the specified command.
        /// </summary>
        /// <param name="cmd">the command</param>
        /// <returns>a parameter binder</returns>
        public abstract IDataParameterBinder MakeParameterBinder(DbCommand cmd);

        /// <summary>
        ///     Gets an object that describes how the specified runtime type is translated
        ///     to the underlying database's type system.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract DbTypeTranslation TranslateRuntimeType(Type type);

        /// <summary>
        ///     When implemented by subclass, begins asynchronous execution of the specified non-query command.
        /// </summary>
        /// <param name="command">the command</param>
        /// <param name="callback">an async callback object</param>
        /// <param name="stateObject">an opaque state object returned to the async callback when the command completes.</param>
        /// <returns>an async result where the asynchrounous execution can be monitored</returns>
        /// <exception cref="NotImplementedException">thrown if the subclass doesn't support async execution</exception>
        /// <seealso cref="EndExecuteNonQuery" />
        public virtual IAsyncResult BeginExecuteNonQuery(DbCommand command, AsyncCallback callback, Object stateObject)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Ensures(Contract.Result<IAsyncResult>() != null);

            throw new NotImplementedException();
        }

        /// <summary>
        ///     When implemented by subclass, begins asynchronous execution of the specified query command.
        /// </summary>
        /// <param name="command">the command</param>
        /// <param name="callback">an async callback object</param>
        /// <param name="stateObject">an opaque state object returned to the async callback when the command completes.</param>
        /// <returns>an async result where the asynchrounous execution can be monitored</returns>
        /// <exception cref="NotImplementedException">thrown if the subclass doesn't support async execution</exception>
        /// <seealso cref="EndExecuteReader" />
        public virtual IAsyncResult BeginExecuteReader(DbCommand command, AsyncCallback callback, Object stateObject)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Ensures(Contract.Result<IAsyncResult>() != null);

            throw new NotImplementedException();
        }

        /// <summary>
        ///     Determines if the specified exception trapped by user code may allow the
        ///     transaction to be retried (such as connection failture, etc).
        /// </summary>
        /// <param name="ex">the observed exception</param>
        /// <returns><em>true</em> if the transaction may be retried; otherwise <em>false</em>.</returns>
        public virtual bool CanRetryTransaction(Exception ex)
        {
            return false;
        }

        /// <summary>
        ///     Determines if a catalog (database) exists on the connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="catalog"></param>
        public virtual bool CatalogExists(DbConnection connection, string catalog)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     When implemented by subclass, ends asynchronous execution of the specified non-query command.
        /// </summary>
        /// <param name="command">the command</param>
        /// <param name="ar">the async result returned from a prior call to <see cref="BeginExecuteNonQuery" /></param>
        /// <returns>the number of objects affected by the operation</returns>
        /// <seealso cref="BeginExecuteNonQuery" />
        public virtual int EndExecuteNonQuery(DbCommand command, IAsyncResult ar)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentNullException>(ar != null);

            throw new NotImplementedException();
        }

        /// <summary>
        ///     When implemented by subclass, ends asynchronous execution of the specified query command.
        /// </summary>
        /// <param name="command">the command</param>
        /// <param name="ar">the async result returned from a prior call to <see cref="BeginExecuteReader" /></param>
        /// <returns>a data reader</returns>
        /// <seealso cref="BeginExecuteReader" />
        public virtual DbDataReader EndExecuteReader(DbCommand command, IAsyncResult ar)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentNullException>(ar != null);
            Contract.Ensures(Contract.Result<DbDataReader>() != null);

            throw new NotImplementedException();
        }

        /// <summary>
        ///     Wraps the specified name in quotes according to the underlying database's naming rules.
        /// </summary>
        /// <param name="name">the database object name</param>
        /// <returns></returns>
        public virtual string QuoteObjectName(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentException>(name.Length > 0);
            Contract.Ensures(Contract.Result<string>() != null);

            if (name[0] == '['
                && name[name.Length - 1] == ']')
            {
                return name;
            }

            return String.Concat('[', name.Replace("]", "]]"), ']');
        }

        /// <summary>
        ///     Determins if the connection supports asynchronous processing.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public virtual bool SupportsAsynchronousProcessing(DbConnection connection)
        {
            return false;
        }

        /// <summary>
        ///     Determins if the connection supports multiple active result sets.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public virtual bool SupportsMultipleActiveResultSets(DbConnection connection)
        {
            return false;
        }

        /// <summary>
        ///     Determins if the specified catalog and schema exist on the specified connection.
        /// </summary>
        /// <param name="connection">the connection</param>
        /// <param name="catalog">the catalog name</param>
        /// <param name="schema">the schema name</param>
        /// <returns><em>true</em> if the schema exists in the specified catalog; otherwise <em>false</em></returns>
        public virtual bool SchemaExists(DbConnection connection, string catalog, string schema)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentNullException>(catalog != null);
            Contract.Requires<ArgumentException>(catalog.Length > 0);
            Contract.Requires<ArgumentNullException>(schema != null);
            Contract.Requires<ArgumentException>(schema.Length > 0);

            return PerformSchemaExists(connection, catalog, schema);
        }

        /// <summary>
        ///     Determins if the specified catalog and schema exist on the specified connection.
        /// </summary>
        /// <param name="connection">the connection</param>
        /// <param name="catalog">the catalog name</param>
        /// <param name="schema">the schema name</param>
        /// <returns><em>true</em> if the schema exists in the specified catalog; otherwise <em>false</em></returns>
        protected virtual bool PerformSchemaExists(DbConnection connection, string catalog, string schema)
        {
            var commandText = Util.NonBlockingLazyInitializeVolatile(ref _commandTextSchemaExists,
                () => String.Format(@"SELECT COUNT(SCHEMA_NAME) 
FROM INFORMATION_SCHEMA.SCHEMATA 
WHERE CATALOG_NAME = {0}
  AND SCHEMA_NAME = {1}",
                    FormatParameterName("catalog"),
                    FormatParameterName("schema")));

            return ExecuteEnumerable(connection, commandText,
                binder =>
                {
                    binder.DefineAndBindParameter("catalog", catalog);
                    binder.DefineAndBindParameter("schema", schema);
                },
                record => record.GetInt32(0))
                       .First() > 0;
        }

        /// <summary>
        ///     Creates the specified schema in the specified catalog.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="catalog"></param>
        /// <param name="schema"></param>
        public void CreateSchema(DbConnection connection, string catalog, string schema)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentNullException>(catalog != null);
            Contract.Requires<ArgumentException>(catalog.Length > 0);
            Contract.Requires<ArgumentNullException>(schema != null);
            Contract.Requires<ArgumentException>(schema.Length > 0);

            PerformCreateSchema(connection, catalog, schema);
        }

        /// <summary>
        ///     When implemented by subclass; creates the specified schema in the specified catalog.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="catalog"></param>
        /// <param name="schema"></param>
        protected abstract void PerformCreateSchema(DbConnection connection, string catalog, string schema);

        /// <summary>
        ///     Determines if the specified function exists on the specified database connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="catalog"></param>
        /// <param name="schema"></param>
        /// <param name="fun"></param>
        /// <returns></returns>
        public bool FunctionExists(DbConnection connection, string catalog, string schema, string fun)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentNullException>(catalog != null);
            Contract.Requires<ArgumentException>(catalog.Length > 0);
            Contract.Requires<ArgumentNullException>(schema != null);
            Contract.Requires<ArgumentException>(schema.Length > 0);
            Contract.Requires<ArgumentNullException>(fun != null);
            Contract.Requires<ArgumentException>(fun.Length > 0);

            return PerformFunctionExists(connection, catalog, schema, fun);
        }

        /// <summary>
        ///     Determines if the specified stored procedure exists on the specified database connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="catalog"></param>
        /// <param name="schema"></param>
        /// <param name="fun"></param>
        /// <returns></returns>
        protected virtual bool PerformFunctionExists(DbConnection connection, string catalog, string schema,
            string fun)
        {
            var commandText = Util.NonBlockingLazyInitializeVolatile(ref _commandTextFunctionExists,
                () => String.Format(@"SELECT COUNT(ROUTINE_NAME)
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_CATALOG = {0}
	AND ROUTINE_SCHEMA = {1}
	AND ROUTINE_NAME = {2}
	AND ROUTINE_TYPE = 'FUNCTION'",
                    FormatParameterName("catalog"),
                    FormatParameterName("schema"),
                    FormatParameterName("fun")));

            return ExecuteEnumerable(connection, commandText,
                binder =>
                {
                    binder.DefineAndBindParameter("catalog", catalog);
                    binder.DefineAndBindParameter("schema", schema);
                    binder.DefineAndBindParameter("fun", fun);
                },
                record => record.GetInt32(0))
                       .First() > 0;
        }

        /// <summary>
        ///     Determines if the specified stored procedure exists on the specified database connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="catalog"></param>
        /// <param name="schema"></param>
        /// <param name="storedProcedure"></param>
        /// <returns></returns>
        public bool StoredProcedureExists(DbConnection connection, string catalog, string schema, string storedProcedure)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentNullException>(catalog != null);
            Contract.Requires<ArgumentException>(catalog.Length > 0);
            Contract.Requires<ArgumentNullException>(schema != null);
            Contract.Requires<ArgumentException>(schema.Length > 0);
            Contract.Requires<ArgumentNullException>(storedProcedure != null);
            Contract.Requires<ArgumentException>(storedProcedure.Length > 0);

            return PerformStoredProcedureExists(connection, catalog, schema, storedProcedure);
        }

        /// <summary>
        ///     Determines if the specified stored procedure exists on the specified database connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="catalog"></param>
        /// <param name="schema"></param>
        /// <param name="storedProcedure"></param>
        /// <returns></returns>
        protected virtual bool PerformStoredProcedureExists(DbConnection connection, string catalog, string schema,
            string storedProcedure)
        {
            var commandText = Util.NonBlockingLazyInitializeVolatile(ref _commandTextStoredProcExists,
                () => String.Format(@"SELECT COUNT(ROUTINE_NAME)
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_CATALOG = {0}
	AND ROUTINE_SCHEMA = {1}
	AND ROUTINE_NAME = {2}
	AND ROUTINE_TYPE = 'PROCEDURE'",
                    FormatParameterName("catalog"),
                    FormatParameterName("schema"),
                    FormatParameterName("storedProcedure")));

            return ExecuteEnumerable(connection, commandText,
                binder =>
                {
                    binder.DefineAndBindParameter("catalog", catalog);
                    binder.DefineAndBindParameter("schema", schema);
                    binder.DefineAndBindParameter("storedProcedure", storedProcedure);
                },
                record => record.GetInt32(0))
                       .First() > 0;
        }

        /// <summary>
        ///     Determines if the specified table exists on the specified database connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="catalog"></param>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool TableExists(DbConnection connection, string catalog, string schema, string table)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentNullException>(catalog != null);
            Contract.Requires<ArgumentException>(catalog.Length > 0);
            Contract.Requires<ArgumentNullException>(schema != null);
            Contract.Requires<ArgumentException>(schema.Length > 0);
            Contract.Requires<ArgumentNullException>(table != null);
            Contract.Requires<ArgumentException>(table.Length > 0);

            return PerformTableExists(connection, catalog, schema, table);
        }

        /// <summary>
        ///     Determines if the specified table exists on the specified database connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="catalog"></param>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        protected virtual bool PerformTableExists(DbConnection connection, string catalog, string schema, string table)
        {
            var commandText = Util.NonBlockingLazyInitializeVolatile(ref _commandTextTableExists,
                () => String.Format(@"SELECT COUNT(TABLE_NAME)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_CATALOG = {0}
	AND TABLE_SCHEMA = {1}
	AND TABLE_NAME = {2}
	AND TABLE_TYPE = 'BASE TABLE'",
                    FormatParameterName("catalog"),
                    FormatParameterName("schema"),
                    FormatParameterName("table")));

            return ExecuteEnumerable(connection, commandText,
                binder =>
                {
                    binder.DefineAndBindParameter("catalog", catalog);
                    binder.DefineAndBindParameter("schema", schema);
                    binder.DefineAndBindParameter("table", table);
                },
                record => record.GetInt32(0))
                       .First() > 0;
        }

        /// <summary>
        ///     Determines if the specified table exists on the specified database connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="catalog"></param>
        /// <param name="schema"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public bool ViewExists(DbConnection connection, string catalog, string schema, string view)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentNullException>(catalog != null);
            Contract.Requires<ArgumentException>(catalog.Length > 0);
            Contract.Requires<ArgumentNullException>(schema != null);
            Contract.Requires<ArgumentException>(schema.Length > 0);
            Contract.Requires<ArgumentNullException>(view != null);
            Contract.Requires<ArgumentException>(view.Length > 0);

            return PerformViewExists(connection, catalog, schema, view);
        }

        /// <summary>
        ///     Determines if the specified table exists on the specified database connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="catalog"></param>
        /// <param name="schema"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        protected virtual bool PerformViewExists(DbConnection connection, string catalog, string schema, string view)
        {
            var commandText = Util.NonBlockingLazyInitializeVolatile(ref _commandTextViewExists,
                () => String.Format(@"SELECT COUNT(TABLE_NAME)
FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_CATALOG = {0}
	AND TABLE_SCHEMA = {1}
	AND TABLE_NAME = {2}",
                    FormatParameterName("catalog"),
                    FormatParameterName("schema"),
                    FormatParameterName("view")));

            return ExecuteEnumerable(connection, commandText,
                binder =>
                {
                    binder.DefineAndBindParameter("catalog", catalog);
                    binder.DefineAndBindParameter("schema", schema);
                    binder.DefineAndBindParameter("view", view);
                },
                record => record.GetInt32(0))
                       .First() > 0;
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
        public virtual IEnumerable<T> ExecuteEnumerable<T>(DbConnection connection, string command,
            Action<IDataParameterBinder> binder, Func<DbDataReader, T> transform)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Requires<ArgumentException>(command.Length > 0);
            Contract.Requires<ArgumentNullException>(transform != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            using (var cmd = connection.CreateCommand(command))
            {
                if (binder != null)
                {
                    var b = MakeParameterBinder(cmd);
                    binder(b);
                }
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
        ///     Executes a non-query command on the specified connection.
        /// </summary>
        /// <param name="connection">the connection</param>
        /// <param name="prepareCommand">callback method that prepares the command</param>
        /// <param name="binder">an optional callback that will bind the command's parameters</param>
        /// <param name="postCommand">an optional callback invoked after the command completes.</param>
        /// <returns></returns>
        public virtual int ExecuteNonQuery(DbConnection connection,
            Action<DbCommand> prepareCommand,
            Action<IDataParameterBinder> binder,
            Action<DbCommand, IDataParameterBinder, int> postCommand)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(prepareCommand != null);

            using (var cmd = connection.CreateCommand())
            {
                prepareCommand(cmd);
                // Ensure the callback established the command's text...
                Contract.Assert(!String.IsNullOrEmpty(cmd.CommandText));

                var b = MakeParameterBinder(cmd);
                if (binder != null)
                {
                    binder(b);
                }
                var res = cmd.ExecuteNonQuery();
                if (postCommand != null)
                {
                    postCommand(cmd, b, res);
                }
                return res;
            }
        }

        /// <summary>
        ///     Binds and executes the specified command for each of the specified data items.
        /// </summary>
        /// <param name="connection">the connection upon which the command will be executed</param>
        /// <param name="data">the data items used in the batch</param>
        /// <param name="setup">callback method that called to setup the command before it is prepared</param>
        /// <param name="binder">callback method that binds the command's parameters for each item of data</param>
        /// <param name="observer">an optional callback method for observing the result for each item of data</param>
        /// <returns>an enumerable over the items' results</returns>
        public IEnumerable<int> ImmediateExecuteNonQueryBatch<TData>(DbConnection connection,
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

            using (var cmd = connection.CreateCommand())
            {
                var b = MakeParameterBinder(cmd);
                setup(cmd, b);
                Contract.Assert(!String.IsNullOrEmpty(cmd.CommandText));
                cmd.Prepare();

                var res = new List<int>();
                foreach (var item in data)
                {
                    binder(cmd, b, item);
                    var itemRes = cmd.ExecuteNonQuery();
                    res.Add(itemRes);
                    if (observer != null)
                    {
                        observer(cmd, b, item, itemRes);
                    }
                }
                return res;
            }
        }

        /// <summary>
        ///     Executes a non-query command on the specified connection.
        /// </summary>
        /// <param name="connection">the connection</param>
        /// <param name="prepareCommand">a callback that will prepare the command</param>
        /// <param name="postCommand">an optional callback invoked after the command completes.</param>
        /// <returns></returns>
        protected virtual int ExecuteNonQuery(DbConnection connection,
            Action<DbCommand> prepareCommand,
            Action<DbCommand, int> postCommand)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<InvalidOperationException>(connection.State.HasFlag(ConnectionState.Open));
            Contract.Requires<ArgumentNullException>(prepareCommand != null);

            using (var cmd = connection.CreateCommand())
            {
                prepareCommand(cmd);
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

        internal MappedDbTypeEmitter GetDbTypeEmitter(IMapping mapping, ColumnMapping column)
        {
            var type = column.RuntimeType;
            if (column.IsReference
                && column.ReferenceTargetMember != null)
            {
                type = column.ReferenceTargetMember.GetTypeOfValue();
            }
            return GetDbTypeEmitter(mapping, type);
        }

        internal MappedDbTypeEmitter GetDbTypeEmitter(IMapping mapping, Type type)
        {
            MappedDbTypeEmitter emitter;
            if (!_emitters.TryGetValue(type, out emitter))
            {
                if (type.IsEnum)
                {
                    var etype = Enum.GetUnderlyingType(type);
                    Type emitterType;
                    switch (Type.GetTypeCode(etype))
                    {
                        case TypeCode.Int16:
                            emitterType = MakeEnumAsInt16Emitter(type);
                            break;
                        case TypeCode.Int32:
                            emitterType = MakeEnumAsInt32Emitter(type);
                            break;
                        default:
                            throw new NotSupportedException(String.Concat("Unable to map enum type ",
                                type.GetReadableSimpleName(),
                                " to DbType due to unsupported underlying type: ", etype.GetReadableSimpleName(), "."));
                    }
                    return (MappedDbTypeEmitter)Activator.CreateInstance(emitterType, true);
                }
                emitter = GetMissingDbTypeEmitter(mapping, type);
            }
            return emitter;
        }

        /// <summary>
        ///     Creates a mapped emitter for the specified enum type.
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        protected virtual Type MakeEnumAsInt32Emitter(Type enumType)
        {
            Contract.Requires<ArgumentException>(enumType.IsEnum);
            Contract.Ensures(Contract.Result<Type>() != null);

            return typeof(MappedEmumAsInt32Emitter<>).MakeGenericType(enumType);
        }

        /// <summary>
        ///     Creates a mapped emitter for the specified enum type.
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        protected virtual Type MakeEnumAsInt16Emitter(Type enumType)
        {
            Contract.Requires<ArgumentException>(enumType.IsEnum);
            Contract.Ensures(Contract.Result<Type>() != null);

            return typeof(MappedEmumAsInt16Emitter<>).MakeGenericType(enumType);
        }

        /// <summary>
        ///     When implemented by subclass; gets a mapped type emitter for the specified type.
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual MappedDbTypeEmitter GetMissingDbTypeEmitter(IMapping mapping, Type type)
        {
            throw new NotImplementedException(String.Concat("There is no mapping for `", type.GetReadableFullName(),
                "` registered for the underlying DbProvider."));
        }

        /// <summary>
        ///     When implemented by subclass; adds a create schema command to the specified batch.
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="schemaName"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void EmitCreateSchema(StringBuilder batch, string schemaName)
        {
            throw new NotImplementedException();
        }
    }
}