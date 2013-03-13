#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using FlitBit.Core;
using FlitBit.Core.Parallel;
using FlitBit.Data.CodeContracts;
using FlitBit.Data.Properties;

namespace FlitBit.Data
{
	[ContractClass(typeof(ContractForIDbExecutable))]
	public interface IDbExecutable : IInterrogateDisposable
	{
		CommandBehaviors Behaviors { get; }
		string CommandText { get; }
		CommandType CommandType { get; }
		int CommandTimeout { get; }
		string ConnectionName { get; }
		bool IsExecutableCommand { get; }
		IDbContext Context { get; }
		IDataParameterBinder ParameterBinder { get; }
		IEnumerable<ParameterBinding> Bindings { get; }

		/// <summary>
		///   Creates an instance of the executable on the connection given.
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		IDbExecutable CreateOnConnection(DbConnection connection);

		IDbExecutable CreateOnConnection(string connection);

		IDbExecutable ExcludeBehaviors(CommandBehaviors behaviors);
		IEnumerable<IDataRecord> ExecuteEnumerable();
		int ExecuteNonQuery();

		void ExecuteNonQuery(Continuation<DbResult<int>> continuation);
		void ExecuteReader(Action<DbResult<DbDataReader>> action);
		void ExecuteReader(Continuation<DbResult<DbDataReader>> continuation);
		T ExecuteScalar<T>();
		void ExecuteScalar<T>(Continuation<DbResult<T>> continuation);
		IDbExecutable IncludeBehaviors(CommandBehaviors behaviors);
	}

	namespace CodeContracts
	{
		/// <summary>
		///   CodeContracts Class for IDbExecutable
		/// </summary>
		[ContractClassFor(typeof(IDbExecutable))]
		internal abstract class ContractForIDbExecutable : IDbExecutable
		{
			public CommandBehaviors Behaviors
			{
				get { throw new NotImplementedException(); }
			}

			public string CommandText
			{
				get { throw new NotImplementedException(); }
			}

			public CommandType CommandType
			{
				get { throw new NotImplementedException(); }
			}

			public string ConnectionName
			{
				get { throw new NotImplementedException(); }
			}

			public bool IsExecutableCommand
			{
				get { throw new NotImplementedException(); }
			}

			public IDbContext Context
			{
				get { throw new NotImplementedException(); }
			}

			public IDbExecutable ExcludeBehaviors(CommandBehaviors behaviors) { throw new NotImplementedException(); }

			public IDbExecutable IncludeBehaviors(CommandBehaviors behaviors) { throw new NotImplementedException(); }

			public int ExecuteNonQuery()
			{
				Contract.Requires<InvalidOperationException>(this.IsExecutableCommand, Resources.Chk_CannotExecutCommandDefinition);

				throw new NotImplementedException();
			}

			public void ExecuteReader(Action<DbResult<DbDataReader>> action)
			{
				Contract.Requires<InvalidOperationException>(action != null);
				Contract.Requires<InvalidOperationException>(this.IsExecutableCommand, Resources.Chk_CannotExecutCommandDefinition);

				throw new NotImplementedException();
			}

			public IEnumerable<IDataRecord> ExecuteEnumerable()
			{
				Contract.Requires<InvalidOperationException>(this.IsExecutableCommand, Resources.Chk_CannotExecutCommandDefinition);
				Contract.Ensures(Contract.Result<IEnumerable<IDataRecord>>() != null);

				throw new NotImplementedException();
			}

			public T ExecuteScalar<T>()
			{
				Contract.Requires<InvalidOperationException>(this.IsExecutableCommand, Resources.Chk_CannotExecutCommandDefinition);

				throw new NotImplementedException();
			}

			public void ExecuteNonQuery(Continuation<DbResult<int>> continuation)
			{
				Contract.Requires<InvalidOperationException>(this.IsExecutableCommand, Resources.Chk_CannotExecutCommandDefinition);

				throw new NotImplementedException();
			}

			public void ExecuteReader(Continuation<DbResult<DbDataReader>> continuation)
			{
				Contract.Requires<InvalidOperationException>(this.IsExecutableCommand, Resources.Chk_CannotExecutCommandDefinition);
				Contract.Requires<ArgumentNullException>(continuation != null);

				throw new NotImplementedException();
			}

			public void ExecuteScalar<T>(Continuation<DbResult<T>> continuation)
			{
				Contract.Requires<InvalidOperationException>(this.IsExecutableCommand, Resources.Chk_CannotExecutCommandDefinition);

				throw new NotImplementedException();
			}

			public IDbExecutable CreateOnConnection(DbConnection connection)
			{
				Contract.Requires<ArgumentNullException>(connection != null);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);

				throw new NotImplementedException();
			}

			public IDbExecutable CreateOnConnection(string connection)
			{
				Contract.Requires<ArgumentNullException>(connection != null);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);

				throw new NotImplementedException();
			}

			public bool IsDisposed
			{
				get { throw new NotImplementedException(); }
			}

			public void Dispose() { throw new NotImplementedException(); }

			public IDataParameterBinder ParameterBinder
			{
				get
				{
					Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

					throw new NotImplementedException();
				}
			}

			public int CommandTimeout
			{
				get { throw new NotImplementedException(); }
			}

			public IEnumerable<ParameterBinding> Bindings
			{
				get
				{
					Contract.Ensures(Contract.Result<IEnumerable<ParameterBinding>>() != null);

					throw new NotImplementedException();
				}
			}
		}
	}
}