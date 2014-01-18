#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using FlitBit.Core;
using FlitBit.Core.Parallel;
using FlitBit.Data.Properties;

namespace FlitBit.Data
{
	public partial class DbExecutable<THelper, TConnection, TCommand, TDbDataReader, TDataBinder>
		: Disposable, IDbExecutable
		where THelper : DbProviderHelper, new()
		where TConnection : DbConnection
		where TCommand : DbCommand, new()
		where TDbDataReader : DbDataReader
		where TDataBinder : class, IDataParameterBinder, new()
	{
		CommandBehaviors _behavior;
		TDataBinder _binder;
		TCommand _command;
		TConnection _connection;
		IDbExecutable _definition;
		bool _ourConnection;

		public DbExecutable()
		{}

		public DbExecutable(string cmdText)
		{
			this.CommandText = cmdText;
		}

		public DbExecutable(string cmdText, CommandType cmdType)
		{
			this.CommandText = cmdText;
			this.CommandType = cmdType;
		}

		public DbExecutable(string cmdText, CommandType cmdType, int cmdTimeout)
		{
			this.CommandText = cmdText;
			this.CommandType = cmdType;
			this.CommandTimeout = cmdTimeout;
		}

		public DbExecutable(string connectionName, string cmdText)
		{
			this.ConnectionName = connectionName;
			this.CommandText = cmdText;
		}

		public DbExecutable(string connectionName, string cmdText, CommandType cmdType)
		{
			this.ConnectionName = connectionName;
			this.CommandText = cmdText;
			this.CommandType = cmdType;
		}

		public DbExecutable(string connectionName, string cmdText, CommandType cmdType, int cmdTimeout)
		{
			this.ConnectionName = connectionName;
			this.CommandText = cmdText;
			this.CommandType = cmdType;
			this.CommandTimeout = cmdTimeout;
		}

		public DbExecutable(string connectionName, IDbExecutable definition)
		{
			Contract.Requires<ArgumentNullException>(connectionName != null);
			Contract.Requires<ArgumentException>(connectionName.Length > 0);
			Contract.Requires<ArgumentNullException>(definition != null);

			_definition = definition;
			this.ConnectionName = connectionName;
			this.CommandText = definition.CommandText;
			this.CommandType = definition.CommandType;
			var bindings = definition.Bindings;
			if (bindings.Any())
			{
				_binder = new TDataBinder();
				_binder.Initialize(bindings);
			}
		}

		public DbExecutable(TConnection connection, IDbExecutable definition)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(definition != null);

			_definition = definition;
			_connection = connection;
			connection.Disposed += connection_Disposed;
			this.CommandText = definition.CommandText;
			this.CommandType = definition.CommandType;
			this.CommandTimeout = definition.CommandTimeout;
			var bindings = definition.Bindings;
			if (bindings.Any())
			{
				_binder = new TDataBinder();
				_binder.Initialize(bindings);
			}
		}

		public DbExecutable(TConnection connection, string cmdText)
		{
			_connection = connection;
			connection.Disposed += connection_Disposed;
			this.CommandText = cmdText;
		}

		public DbExecutable(TConnection connection, string cmdText, CommandType cmdType)
			: this(connection, cmdText)
		{
			this.CommandType = cmdType;
		}

		public DbExecutable(TConnection connection, string cmdText, CommandType cmdType, int cmdTimeout)
			: this(connection, cmdText, cmdType)
		{
			this.CommandTimeout = cmdTimeout;
		}

		public void PostProcessDbCommand(DbCommand command)
		{}

		public bool PrepareDbCommandForExecute(DbCommand command)
		{
			Contract.Requires<ArgumentNullException>(command != null);

			if (_binder != null)
			{
				return _binder.PrepareDbCommand(command);
			}
			return false;
		}

		protected override bool PerformDispose(bool disposing)
		{
			if (disposing)
			{
				Util.Dispose(ref _command);
				if (_ourConnection)
				{
					_connection.Close();
				}
			}
			if (_connection != null)
			{
				_connection.Disposed -= connection_Disposed;
			}
			return true;
		}

		protected TCommand MakeDbCommand()
		{
			Contract.Ensures(Contract.Result<TCommand>() != null);
			if (_command != null)
			{
				throw new InvalidOperationException(Resources.Err_OverlappingUseOfExecutable);
			}

			_command = new TCommand();
			_command.Connection = MakeDbConnection()
				.EnsureConnectionIsOpen();
			_command.CommandText = CommandText;
			_command.CommandType = CommandType;
			if (this.CommandTimeout > 0)
			{
				_command.CommandTimeout = this.CommandTimeout;
			}
			_command.Disposed += (sender, e) => { _command = null; };
			return _command;
		}

		protected TConnection MakeDbConnection()
		{
			Contract.Ensures(Contract.Result<TConnection>() != null);

			if (_connection == null)
			{
				var context = DbContext.Current;
				if (context != null)
				{
					if ((_behavior & CommandBehaviors.ShareConnectionIfAvailable) == CommandBehaviors.ShareConnectionIfAvailable)
					{
						_connection = context.SharedOrNewConnection<TConnection>(this.ConnectionName);
					}
					else
					{
						_ourConnection = true;
						_connection = context.NewConnection<TConnection>(this.ConnectionName);
						_connection.Disposed += connection_Disposed;
					}
				}
				else
				{
					_ourConnection = true;
					_connection = DbExtensions.CreateConnection<TConnection>(this.ConnectionName);
				}
			}
			return _connection;
		}

		void connection_Disposed(object sender, EventArgs e)
		{
			Util.Dispose(ref _command);
			_connection = null;
		}

		#region IDbExecutable Members

		/// <summary>
		///   Name of the connection string used by this command.
		/// </summary>
		public string ConnectionName { get; private set; }

		/// <summary>
		///   The command's type (as executed against the underlying DB).
		/// </summary>
		public string CommandText { get; private set; }

		/// <summary>
		///   The command's type.
		/// </summary>
		public CommandType CommandType { get; private set; }

		/// <summary>
		///   The timeout period for the command.
		/// </summary>
		public int CommandTimeout { get; private set; }

		/// <summary>
		///   The command's db context.
		/// </summary>
		public IDbContext Context { get; private set; }

		/// <summary>
		///   Indicates whether the command is executable.
		/// </summary>
		public bool IsExecutableCommand { get { return _definition != null || _connection != null; } }

		/// <summary>
		///   Indicates the command's behaviors.
		/// </summary>
		public CommandBehaviors Behaviors { get { return _behavior; } }

		/// <summary>
		///   Gets the command's parameter bindings.
		/// </summary>
		public IDataParameterBinder ParameterBinder { get { return Util.NonBlockingLazyInitializeVolatile(ref _binder); } }

		/// <summary>
		///   Gets the command's parameter bindings.
		/// </summary>
		public IEnumerable<ParameterBinding> Bindings
		{
			get
			{
				return (_binder != null)
					? _binder.Bindings
					: Enumerable.Empty<ParameterBinding>();
			}
		}

	  public Task<T> ExecuteScalarAsync<T>() { throw new NotImplementedException(); }

	  /// <summary>
		///   Ensures the command includes the given behaviors.
		/// </summary>
		/// <param name="behaviors">behaviors being included</param>
		/// <returns></returns>
		public IDbExecutable IncludeBehaviors(CommandBehaviors behaviors)
		{
			_behavior |= behaviors;
			return this;
		}

		/// <summary>
		///   Ensures the command DOES NOT include the given behaviors.
		/// </summary>
		/// <param name="behaviors">behaviors being excluded</param>
		public IDbExecutable ExcludeBehaviors(CommandBehaviors behaviors)
		{
			_behavior &= (~behaviors);
			return this;
		}

		/// <summary>
		///   Creates an executable command, current command is used as a command definition).
		/// </summary>
		/// <param name="connectionName"></param>
		/// <returns></returns>
		public IDbExecutable CreateOnConnection(string connectionName)
		{
			return new DbExecutable<THelper, TConnection, TCommand, TDbDataReader, TDataBinder>(connectionName, this);
		}

		/// <summary>
		///   Creates an executable command, current command is used as a command definition).
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		public IDbExecutable CreateOnConnection(DbConnection connection)
		{
			if (!typeof(TConnection).IsInstanceOfType(connection))
			{
				throw new InvalidOperationException(String.Concat(Resources.Err_TypeNotSupported,
																													connection.GetType()
																																		.GetReadableSimpleName()));
			}

			return new DbExecutable<THelper, TConnection, TCommand, TDbDataReader, TDataBinder>((TConnection) connection, this);
		}

		public int ExecuteNonQuery()
		{
			using (var cmd = MakeDbCommand())
			{
				var needsPostProcessing = PrepareDbCommandForExecute(cmd);
				var res = cmd.ExecuteNonQuery();
				if (needsPostProcessing)
				{
					PostProcessDbCommand(cmd);
				}
				return res;
			}
		}

		public void ExecuteReader(Action<DbResult<DbDataReader>> action)
		{
			using (var cmd = MakeDbCommand())
			{
				var needsPostProcessing = PrepareDbCommandForExecute(cmd);

				using (var reader = cmd.ExecuteReader())
				{
					if (needsPostProcessing)
					{
						PostProcessDbCommand(cmd);
					}
					action(new DbResult<DbDataReader>(this, reader));
				}
			}
		}

		public IEnumerable<IDataRecord> ExecuteEnumerable()
		{
			using (var cmd = MakeDbCommand())
			{
				var needsPostProcessing = PrepareDbCommandForExecute(cmd);
				var reader = cmd.ExecuteReader();
				try
				{
					if (needsPostProcessing)
					{
						PostProcessDbCommand(cmd);
					}
					while (reader.Read())
					{
						yield return reader;
					}
				}
				finally
				{
					reader.Dispose();
				}
			}
		}

		public T ExecuteScalar<T>()
		{
			using (var cmd = MakeDbCommand())
			{
				var needsPostProcessing = PrepareDbCommandForExecute(cmd);
				var res = (T) cmd.ExecuteScalar();
				if (needsPostProcessing)
				{
					PostProcessDbCommand(cmd);
				}
				return res;
			}
		}

		public void ExecuteNonQuery(Continuation<DbResult<int>> continuation)
		{
			var cn = MakeDbConnection();
			var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
			if (helper.SupportsAsynchronousProcessing(cn))
			{
				var cmd = MakeDbCommand();
				var needsPostProcessing = PrepareDbCommandForExecute(cmd);
				var ar = helper.BeginExecuteReader(cmd,
				  res =>
				  {
            if (!res.CompletedSynchronously)
				    try
				    {
				      continuation(null, new DbResult<int>(this, helper.EndExecuteNonQuery(cmd, res)));
				    }
				    catch (Exception e)
				    {
				      continuation(e, null);
				    }
				  }, null);
			  if (ar.CompletedSynchronously)
			  {
          try
          {
            continuation(null, new DbResult<int>(this, helper.EndExecuteNonQuery(cmd, ar)));
          }
          catch (Exception e)
          {
            continuation(e, null);
          }
			  }
			}
			else
			{
			  Task.Factory.StartNew(ContextFlow.Capture(() =>
			  {
			    try
			    {
			      continuation(null, new DbResult<int>(this, ExecuteNonQuery()));
			    }
			    catch (Exception e)
			    {
			      continuation(e, null);
			    }
			  }));
			}
		}

		public void ExecuteReader(Continuation<DbResult<DbDataReader>> continuation)
		{
			var cn = MakeDbConnection();
			var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
				var cmd = MakeDbCommand();
				var needsPostProcessing = PrepareDbCommandForExecute(cmd);
			if (helper.SupportsAsynchronousProcessing(cn))
			{
				var ar = helper.BeginExecuteReader(cmd, res =>
				{
				  if (res.CompletedSynchronously) { return; }
				  DbDataReader reader;
				  try
				  {
				    reader = helper.EndExecuteReader(cmd, res);
				  }
				  catch (Exception e)
				  {
				    continuation(e, null);
				    return;
				  }
				  using (reader)
				  {
				    continuation(null, new DbResult<DbDataReader>(this, reader));
				  }
				}, null);
				if (ar.CompletedSynchronously)
				{
          DbDataReader reader;
          try
          {
            reader = helper.EndExecuteReader(cmd, ar);
          }
          catch (Exception e)
          {
            continuation(e, null);
            return;
          }
          using (reader)
          {
            continuation(null, new DbResult<DbDataReader>(this, reader));
          }
				}
			}
			else
			{
			  Task.Factory.StartNew(ContextFlow.Capture(() =>
			  {
			    DbDataReader reader;
			    try
			    {
			      reader = cmd.ExecuteReader();
			    }
			    catch (Exception e)
			    {
			      continuation(e, null);
			      return;
			    }
			    using (reader)
			    {
			      continuation(null, new DbResult<DbDataReader>(this, reader));
			    }
			  }));
			}
		}

		public void ExecuteScalar<T>(Continuation<DbResult<T>> continuation)
		{
		  Task.Factory.StartNew(ContextFlow.Capture(() =>
		  {
		    T res;
		    try
		    {
		      res = ExecuteScalar<T>();
		    }
		    catch (Exception e)
		    {
		      continuation(e, null);
		      return;
		    }
        continuation(null, new DbResult<T>(this, res));
		  }));
		}

	  public Task<int> ExecuteNonQueryAsync()
	  {
	    var cn = MakeDbConnection();
			var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
      var cmd = MakeDbCommand();
      PrepareDbCommandForExecute(cmd);
      if (helper.SupportsAsynchronousProcessing(cn))
	    {
	      var completion = new TaskCompletionSource<int>();
	      helper.BeginExecuteReader(cmd,
	        res =>
	        {
	          try
	          {
	            completion.TrySetResult(helper.EndExecuteNonQuery(cmd, res));
	          }
	          catch (OperationCanceledException)
	          {
	            completion.TrySetCanceled();
	          }
	          catch (Exception exc)
	          {
	            completion.TrySetException(exc);
	          }
	        }, null);
	      return completion.Task;
	    }
	    return Task.FromResult(cmd.ExecuteNonQuery());
	  }

	  public Task<DbDataReader> ExecuteReaderAsync()
	  {
      var cn = MakeDbConnection();
      var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(cn);
      var cmd = MakeDbCommand();
      PrepareDbCommandForExecute(cmd);
      if (helper.SupportsAsynchronousProcessing(cn))
      {
        var completion = new TaskCompletionSource<DbDataReader>();
        helper.BeginExecuteReader(cmd,
          res =>
          {
            try
            {
              completion.TrySetResult(helper.EndExecuteReader(cmd, res));
            }
            catch (OperationCanceledException)
            {
              completion.TrySetCanceled();
            }
            catch (Exception exc)
            {
              completion.TrySetException(exc);
            }
          }, null);
        return completion.Task;
      }
	    return Task.FromResult(cmd.ExecuteReader());
	  }

	  #endregion
	}
}