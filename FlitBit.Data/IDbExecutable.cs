#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Data;
using System.Diagnostics.Contracts;
using FlitBit.Core;
using FlitBit.Core.Parallel;
using FlitBit.Data.Properties;

namespace FlitBit.Data
{
	[ContractClass(typeof(CodeContracts.ContractForIDbExecutable))]
	public interface IDbExecutable: IInterrogateDisposable, IDbContextual
	{
		CommandBehaviors Behaviors { get; }
		string CommandText { get; }
		CommandType CommandType { get; }
		string ConnectionName { get; }
		bool IsExecutableCommand { get; }

		IDbExecutable ExcludeBehaviors(CommandBehaviors behaviors);
		IDbExecutable IncludeBehaviors(CommandBehaviors behaviors);

		int ExecuteNonQuery(IDbContext context);
		IDataReader ExecuteReader(IDbContext context);
		T ExecuteScalar<T>(IDbContext context);
		
		void ExecuteNonQuery(IDbContext context, Continuation<DbResult<int>> continuation);
		void ExecuteReader(IDbContext context, Continuation<DbResult<IDataReader>> continuation);
		void ExecuteScalar<T>(IDbContext context, Continuation<DbResult<T>> continuation);
				
		IDbExecutable CreateOnConnection(IDbConnection connection);
		IDbExecutable CreateOnConnection(string connection);

		IDbExecutable DefineParameter(string name, Type runtimeType);
		IDbExecutable DefineParameter(string name, Type runtimeType, ParameterDirection direction);
		IDbExecutable DefineParameter(string name, Type runtimeType, int length, ParameterDirection direction);
		IDbExecutable DefineParameter(string name, DbType dbType);
		IDbExecutable DefineParameter(string name, DbType dbType, ParameterDirection direction);
		IDbExecutable DefineParameter(string name, DbType dbType, int length);
		IDbExecutable DefineParameter(string name, DbType dbType, int length, ParameterDirection direction);
		IDbExecutable DefineParameter(string name, DbType dbType, int size, byte scale);
		IDbExecutable DefineParameter(string name, DbType dbType, int size, byte scale, ParameterDirection direction);
		IDbExecutable DefineParameter(Func<DbParamDefinition> specializeParam);
		
		IDbExecutable SetParameterValue(string name, bool value);
		IDbExecutable SetParameterValue(string name, byte[] value);
		IDbExecutable SetParameterValue(string name, byte value);
		IDbExecutable SetParameterValue(string name, DateTime value);
		IDbExecutable SetParameterValue(string name, decimal value);
		IDbExecutable SetParameterValue(string name, Double value);
		IDbExecutable SetParameterValue(string name, Guid value);
		IDbExecutable SetParameterValue(string name, Single value);
		
		IDbExecutable SetParameterValue(string name, SByte value);
		IDbExecutable SetParameterValue(string name, string value);
		IDbExecutable SetParameterValue(string name, Int16 value);
		IDbExecutable SetParameterValue(string name, Int32 value);
		IDbExecutable SetParameterValue(string name, Int64 value);
		
		IDbExecutable SetParameterValue(string name, UInt16 value);
		
		IDbExecutable SetParameterValue(string name, UInt32 value);
		
		IDbExecutable SetParameterValue(string name, UInt64 value);
		IDbExecutable SetParameterValue<T>(string name, T value);
		IDbExecutable SetParameterValueAsEnum<E>(string name, E value);

		IDbExecutable SetParameterDbNull(string name);

		int IndexOfParameter(string name);
		
		void GetParameterValueAs<T>(string name, out T value);	

		IDbConnection MakeDbConnection(IDbContext context);
		IDbCommand MakeDbCommand(IDbConnection connection);

		void PrepareDbCommandForExecute();
		void PrepareDbCommandForExecute(Action<IDbExecutable> binder);					
	}

	namespace CodeContracts
	{
		/// <summary>
		/// CodeContracts Class for IDbExecutable
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
				get	{	throw new NotImplementedException(); }
			}

			public bool IsExecutableCommand
			{
				get	{	throw new NotImplementedException(); }
			}

			public IDbExecutable ExcludeBehaviors(CommandBehaviors behaviors)
			{
				throw new NotImplementedException();
			}

			public IDbExecutable IncludeBehaviors(CommandBehaviors behaviors)
			{
				throw new NotImplementedException();
			}

			public int ExecuteNonQuery(IDbContext context)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<InvalidOperationException>(this.IsExecutableCommand, Resources.Chk_CannotExecutCommandDefinition);
				
				throw new NotImplementedException();
			}

			public IDataReader ExecuteReader(IDbContext context)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<InvalidOperationException>(this.IsExecutableCommand, Resources.Chk_CannotExecutCommandDefinition);
				Contract.Ensures(Contract.Result<IDataReader>() != null);

				throw new NotImplementedException();
			}

			public T ExecuteScalar<T>(IDbContext context)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<InvalidOperationException>(this.IsExecutableCommand, Resources.Chk_CannotExecutCommandDefinition);
				
				throw new NotImplementedException();
			}

			public void ExecuteNonQuery(IDbContext context, Continuation<DbResult<int>> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<InvalidOperationException>(this.IsExecutableCommand, Resources.Chk_CannotExecutCommandDefinition);
				
				throw new NotImplementedException();
			}

			public void ExecuteReader(IDbContext context, Continuation<DbResult<IDataReader>> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<InvalidOperationException>(this.IsExecutableCommand, Resources.Chk_CannotExecutCommandDefinition);
				
				throw new NotImplementedException();
			}

			public void ExecuteScalar<T>(IDbContext context, Continuation<DbResult<T>> continuation)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Requires<InvalidOperationException>(this.IsExecutableCommand, Resources.Chk_CannotExecutCommandDefinition);
				
				throw new NotImplementedException();
			}

			public IDbExecutable CreateOnConnection(IDbConnection connection)
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

			public IDbExecutable DefineParameter(string name, Type runtimeType)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Requires<ArgumentNullException>(runtimeType != null);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable DefineParameter(string name, Type runtimeType, ParameterDirection direction)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Requires<ArgumentNullException>(runtimeType != null);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);

				throw new NotImplementedException();
			}

			public IDbExecutable DefineParameter(string name, Type runtimeType, int length, ParameterDirection direction)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Requires<ArgumentNullException>(runtimeType != null);
				Contract.Requires<ArgumentOutOfRangeException>(length > 0);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable DefineParameter(string name, DbType dbType)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);

				throw new NotImplementedException();
			}

			public IDbExecutable DefineParameter(string name, DbType dbType, ParameterDirection direction)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable DefineParameter(string name, DbType dbType, int length)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Requires<ArgumentOutOfRangeException>(length > 0);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable DefineParameter(string name, DbType dbType, int length, ParameterDirection direction)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Requires<ArgumentOutOfRangeException>(length > 0);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable DefineParameter(string name, DbType dbType, int size, byte scale)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Requires<ArgumentOutOfRangeException>(size > 0);
				Contract.Requires<ArgumentOutOfRangeException>(scale >= 0);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable DefineParameter(string name, DbType dbType, int size, byte scale, ParameterDirection direction)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Requires<ArgumentOutOfRangeException>(size > 0);
				Contract.Requires<ArgumentOutOfRangeException>(scale >= 0);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);

				throw new NotImplementedException();
			}

			public IDbExecutable DefineParameter(Func<DbParamDefinition> specializeParam)
			{
				Contract.Requires<ArgumentNullException>(specializeParam != null);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);

				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, bool value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);

				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, byte[] value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);

				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, byte value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, DateTime value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, decimal value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, double value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, Guid value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, float value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, sbyte value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, string value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, short value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, int value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, long value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, ushort value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, uint value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue(string name, ulong value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValue<T>(string name, T value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterValueAsEnum<E>(string name, E value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);

				throw new NotImplementedException();
			}

			public IDbExecutable SetParameterDbNull(string name)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDbExecutable>() != null);
				
				throw new NotImplementedException();
			}

			public int IndexOfParameter(string name)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Ensures(Contract.Result<int>() >= -1);

				throw new NotImplementedException();
			}

			public void GetParameterValueAs<T>(string name, out T value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);

				throw new NotImplementedException();
			}

			public IDbConnection MakeDbConnection(IDbContext context)
			{
				Contract.Requires<ArgumentNullException>(context != null);
				Contract.Ensures(Contract.Result<IDbConnection>() != null);

				throw new NotImplementedException();
			}

			public IDbCommand MakeDbCommand(IDbConnection connection)
			{
				Contract.Requires<ArgumentNullException>(connection != null);
				Contract.Ensures(Contract.Result<IDbCommand>() != null);

				throw new NotImplementedException();
			}

			public void PrepareDbCommandForExecute()
			{
				throw new NotImplementedException();
			}

			public void PrepareDbCommandForExecute(Action<IDbExecutable> binder)
			{
				throw new NotImplementedException();
			}

			public bool IsDisposed
			{
				get { throw new NotImplementedException(); }
			}

			public void Dispose()
			{
				throw new NotImplementedException();
			}
		}
	}
}
