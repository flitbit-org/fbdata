#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using FlitBit.Core;
using FlitBit.Core.Parallel;

namespace FlitBit.Data
{
	public sealed class DbResult<T>
	{
		public DbResult(IDbExecutable exe, T result)
		{
			this.Executable = exe;
			this.Result = result;
		}

		public IDbExecutable Executable { get; private set; }
		public T Result { get; private set; }
	}																		

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
}
