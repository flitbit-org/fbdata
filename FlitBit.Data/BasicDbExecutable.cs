#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Core;

namespace FlitBit.Data
{		 
	public partial class BasicDbExecutable : IDbExecutable
	{				
		IDbContext _context;
		BasicDbExecutable _definition;
		IDbConnection _connection;
		IDbCommand _command;
		ParameterBinding[] _parameters = new ParameterBinding[0];
		CommandBehaviors _behavior;

		public BasicDbExecutable() { }
		public BasicDbExecutable(string connection)
		{
			this.ConnectionName = connection;
		}		
		public BasicDbExecutable(string commandText, CommandType commandType)
		{
			this.CommandText = commandText;
			this.CommandType = commandType;
		}
		public BasicDbExecutable(string connection, string commandText, CommandType commandType)
		{
			this.ConnectionName = connection;
			this.CommandText = commandText;
			this.CommandType = commandType;
		}
		public BasicDbExecutable(string connection, BasicDbExecutable definition)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires(connection.Length > 0);
			Contract.Requires<ArgumentNullException>(definition != null);

			_definition = definition;
			this.ConnectionName = connection;
			this.CommandText = definition.CommandText;
			this.CommandType = definition.CommandType;
			_parameters =definition.Parameters.ToArray();
		}
		public BasicDbExecutable(IDbConnection connection, BasicDbExecutable definition)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(definition != null);

			_definition = definition;
			_connection = connection;
			this.CommandText = definition.CommandText;
			this.CommandType = definition.CommandType;
			_parameters = definition.Parameters.ToArray();
		}
		
		/// <summary>
		/// Name of the connection string used by this command.
		/// </summary>
		public string ConnectionName { get; protected set; }
		/// <summary>
		/// The command's type (as executed against the underlying DB).
		/// </summary>
		public string CommandText { get; protected set; }
		/// <summary>
		/// The command's type.
		/// </summary>
		public CommandType CommandType { get; protected set; }

		/// <summary>
		/// Indicates whether the command is executable.
		/// </summary>
		public bool IsExecutableCommand { get { return _definition != null; } }

		/// <summary>
		/// Indicates the command's behaviors.
		/// </summary>
		public CommandBehaviors Behaviors { get { return _behavior; } }

		/// <summary>
		/// Ensures the command includes the given behaviors.
		/// </summary>
		/// <param name="behaviors">behaviors being included</param>
		/// <returns></returns>
		
		public IDbExecutable IncludeBehaviors(CommandBehaviors behaviors)
		{
			_behavior |= behaviors;
			return this;
		}

		/// <summary>
		/// Ensures the command DOES NOT include the given behaviors.
		/// </summary>
		/// <param name="behaviors">behaviors being excluded</param>
		
		public IDbExecutable ExcludeBehaviors(CommandBehaviors behaviors)
		{
			_behavior &= (~behaviors);
			return this;
		}

		/// <summary>
		/// Gets the command's db context.
		/// </summary>
		public IDbContext Context
		{
			get
			{
				if (_definition == null)
					throw new InvalidOperationException("Command definition does not use a DbContext.");
				return (IDbContext)Util.NonBlockingLazyInitializeVolatile(ref _context, () => new DbContext());
			}
		}

		/// <summary>
		/// Creates an executable command, current command is used as a command definition).
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		public IDbExecutable CreateOnConnection(string connection)
		{
			var provider = DbProviderHelpers.GetDbProviderHelperForDbConnection(connection);
			if (provider == null)
				throw new NotImplementedException(String.Concat("DbProviderHelper not found: ", connection));

			return provider.MakeCommandOnConnection(connection, this);
		}

		/// <summary>
		/// Creates an executable command, current command is used as a command definition).
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		public IDbExecutable CreateOnConnection(IDbConnection connection)
		{
			var provider = DbProviderHelpers.GetDbProviderHelperForDbConnection(connection);
			if (provider == null)
				throw new NotImplementedException(String.Concat("DbProviderHelper not found: ", connection));

			return provider.MakeCommandOnConnection(connection, this);
		}


		public IEnumerable<T> ExecuteTransformAll<T>(IDbContext scope, Func<IDataRecord,T> transform)
		{
			Contract.Assert(scope != null);
			if (!this.IsExecutableCommand)
				throw new InvalidOperationException("Unable to execute a command definition; to execute a command definition use ImmediateExecuteXXXX methods.");

			var cn = MakeDbConnection(scope).EnsureConnectionIsOpen();
			var result = new List<T>();
			using (var cmd = MakeDbCommand(cn))
			{
				PrepareDbCommandForExecute();
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(transform(reader));
					}
				}
			}
			return result;
		}

		public T ExecuteTransformSingleOrDefault<T>(IDbContext context, Func<IDataRecord, T> transform)
		{
			Contract.Assert(context != null);
			if (!this.IsExecutableCommand)
				throw new InvalidOperationException("Unable to execute a command definition; to execute a command definition use ImmediateExecuteXXXX methods.");

			var cn = MakeDbConnection(context).EnsureConnectionIsOpen();
			var result = default(T);
			using (var cmd = MakeDbCommand(cn))
			{
				PrepareDbCommandForExecute();
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						result = transform(reader);
					}
				}
			}
			return result;
		}

		public int ExecuteNonQuery(IDbContext context)
		{
			Contract.Assert(context != null);
			if (!this.IsExecutableCommand)
				throw new InvalidOperationException("Unable to execute a command definition; to execute a command definition use ImmediateExecuteXXXX methods.");

			var cn = MakeDbConnection(context).EnsureConnectionIsOpen();
			using (var cmd = MakeDbCommand(cn))
			{
				PrepareDbCommandForExecute();
				return cmd.ExecuteNonQuery();
			}			
		}

		public void ExecuteReader(IDbContext context, Action<IDbContext, IDataReader> handler)
		{
			Contract.Assert(context != null);
			Contract.Assert(handler != null);

			if (!this.IsExecutableCommand)
				throw new InvalidOperationException("Unable to execute a command definition; to execute a command definition use ImmediateExecuteXXXX methods.");

			var cn = MakeDbConnection(context).EnsureConnectionIsOpen();
			using (var cmd = MakeDbCommand(cn))
			{
				PrepareDbCommandForExecute();
				using (var reader = cmd.ExecuteReader())
				{
					handler(context, reader);
				}
			}
		}

		public int ExecuteNonQuery(IDbContext context, Action<IDbExecutable, int> completion)
		{
			Contract.Assert(context != null);
			Contract.Assert(completion != null);
			if (!this.IsExecutableCommand)
				throw new InvalidOperationException("Unable to execute a command definition; to execute a command definition use ImmediateExecuteXXXX methods.");
			var cn = MakeDbConnection(context).EnsureConnectionIsOpen();
			using (var cmd = MakeDbCommand(cn))
			{
				PrepareDbCommandForExecute();
				var result = cmd.ExecuteNonQuery();
				completion(this, result);
				return result;
			}
		}

		public T ExecuteScalar<T>(IDbContext context)
		{
			Contract.Assert(context != null);
			if (!this.IsExecutableCommand)
				throw new InvalidOperationException("Unable to execute a command definition; to execute a command definition use ImmediateExecuteXXXX methods.");

			var cn = MakeDbConnection(context).EnsureConnectionIsOpen();
			using (var cmd = MakeDbCommand(cn))
			{
				PrepareDbCommandForExecute();
				return (T)cmd.ExecuteScalar();
			}
		}

		public T ExecuteScalar<T>(IDbContext scope, Action<IDbExecutable, T> completion)
		{
			Contract.Assert(scope != null);
			Contract.Assert(completion != null);
			if (!this.IsExecutableCommand)
				throw new InvalidOperationException("Unable to execute a command definition; to execute a command definition use ImmediateExecuteXXXX methods.");
			var cn = MakeDbConnection(scope).EnsureConnectionIsOpen();
			using (var cmd = MakeDbCommand(cn))
			{
				PrepareDbCommandForExecute();
				T result = (T)cmd.ExecuteScalar();
				completion(this, result);
				return result;
			}
		}
		
		public IDbConnection MakeDbConnection(IDbContext context)
		{
			Contract.Assert(context != null);
			if (_connection == null)
			{
				_connection = PerformMakeDbConnection(context);
			}
			return _connection;
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000", Justification = "By design; the disposable type is added to a CleanupScope.")]
		protected virtual IDbConnection PerformMakeDbConnection(IDbContext context)
		{
			Contract.Requires<ArgumentNullException>(context != null);

			if ((_behavior & CommandBehaviors.ShareConnectionIfAvailable) == CommandBehaviors.ShareConnectionIfAvailable)
			{
				return context.NewOrSharedConnection(this.ConnectionName);
			}
			else
			{
				return context.NewConnection(this.ConnectionName);
			}
		}

		public IDbCommand MakeDbCommand(IDbConnection connection)
		{
			Contract.Assert(connection != null);

			if (_command == null)
			{
				_command = PerformMakeDbCommand(connection);
			}
			return _command;
		}
		protected virtual IDbCommand PerformMakeDbCommand(IDbConnection connection)
		{
			return connection.CreateCommand(this.CommandText, this.CommandType);
		}

		public void PrepareDbCommandForExecute(Action<IDbExecutable> binder)
		{
			if (_connection == null) throw new InvalidOperationException("IDbConnection not present");
			if (_command == null) throw new InvalidOperationException("IDbCommand not present");

			if (binder != null) binder(this);
			PrepareDbCommandForExecute();
		}

		public void PrepareDbCommandForExecute()
		{
			if (_connection == null) throw new InvalidOperationException("IDbConnection not present");
			if (_command == null) throw new InvalidOperationException("IDbCommand not present");

			var parms = this.Parameters;
			if (parms.Count() > 0)
			{
				var pp = this.PrepareParametersFromSource(parms);
				PrepareParametersForExecute(_command, pp);
			}			
		}
		
		#region IDisposable pattern
		~BasicDbExecutable()
		{
			Dispose(false);
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			Util.Dispose(ref _context);
		}
		#endregion
	}
}
