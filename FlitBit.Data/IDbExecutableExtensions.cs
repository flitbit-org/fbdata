#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using FlitBit.Core;
using FlitBit.Core.Parallel;
using FlitBit.Data.Properties;
using FlitBit.Emit;

namespace FlitBit.Data
{
	public static class IDbExecutableExtensions
	{
		public static void ExecuteNonQuery(this IDbExecutable exe, Action<IDbExecutable, int> after)
		{
			Contract.Requires<ArgumentNullException>(exe != null);
			Contract.Requires<ArgumentNullException>(after != null);

			after(exe, exe.ExecuteNonQuery());
		}
		
		public static void ExecuteScalar<T>(this IDbExecutable exe,  Action<IDbExecutable, T> after)
		{
			Contract.Requires<ArgumentNullException>(exe != null);
			Contract.Requires<ArgumentNullException>(after != null);

			after(exe, exe.ExecuteScalar<T>());
		}

		public static Completion<DbResult<int>> ExecuteNonQueryWithCompletion(this IDbExecutable exe, Continuation<DbResult<int>> continuation)
		{
			Contract.Requires<ArgumentNullException>(exe != null);

			var result = new Completion<DbResult<int>>(exe);
			exe.ExecuteNonQuery((e, res) =>
			{
				if (e != null) result.MarkFaulted(e);
				else result.MarkCompleted(res);
			});
			if (continuation != null)	result.Continue(continuation);
			return result;
		}

		public static Completion<DbResult<DbDataReader>> ExecuteReaderWithCompletion(this IDbExecutable exe, Continuation<DbResult<DbDataReader>> continuation)
		{
			Contract.Requires<ArgumentNullException>(exe != null);

			var result = new Completion<DbResult<DbDataReader>>(exe);
			exe.ExecuteReader((e, res) =>
			{
				if (e != null) result.MarkFaulted(e);
				else result.MarkCompleted(res);
			});
			if (continuation != null) result.Continue(continuation);
			return result;
		}

		public static Completion<DbResult<T>> ExecuteScalarWithCompletion<T>(this IDbExecutable exe, Continuation<DbResult<T>> continuation)
		{
			Contract.Requires<ArgumentNullException>(exe != null);

			var result = new Completion<DbResult<T>>(exe);
			exe.ExecuteScalar<T>((e, res) =>
			{
				if (e != null) result.MarkFaulted(e);
				else result.MarkCompleted(res);
			});
			if (continuation != null) result.Continue(continuation);
			return result;
		}		

		public static int ImmediateExecuteNonQuery(this IDbExecutable command)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			
			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var instance = command.CreateOnConnection(connection))
			{
				return instance.ExecuteNonQuery();
			}
		}		
		public static void ImmediateExecuteNonQuery(this IDbExecutable command, Action<IDbExecutable, int> completion)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(completion != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var instance = command.CreateOnConnection(connection))
			{
				instance.ExecuteNonQuery(completion);
			}
		}
		public static int ImmediateExecuteNonQuery(this IDbExecutable command, Action<IDataParameterBinder> binder)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(binder != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var instance = command.CreateOnConnection(connection))
			{
				binder((IDataParameterBinder)instance);
				return instance.ExecuteNonQuery();
			}
		}
		
		public static void ImmediateExecuteNonQuery(this IDbExecutable command, Action<IDataParameterBinder> binder, Action<IDbExecutable, int> completion)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(completion != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var instance = command.CreateOnConnection(connection))
			{
				binder((IDataParameterBinder)instance);
				instance.ExecuteNonQuery(completion);
			}
		}

		public static void ImmediateExecuteNonQuery(this IDbExecutable command, string connection, Action<IDbExecutable, int> completion)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires(connection.Length > 0);
			Contract.Requires<ArgumentNullException>(completion != null);

			using (var context = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				instance.ExecuteNonQuery(completion);
			}
		}

		public static void ImmediateExecuteScalar<T>(this IDbExecutable command, Action<IDbExecutable, T> completion)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(completion != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var context = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				instance.ExecuteScalar<T>(completion);
			}
		}

		public static T ImmediateExecuteScalar<T>(this IDbExecutable command, Action<IDataParameterBinder> binder)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(binder != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var instance = command.CreateOnConnection(connection))
			{
				binder((IDataParameterBinder)instance);
				return instance.ExecuteScalar<T>();
			}
		}

		public static void ImmediateExecuteScalar<T>(this IDbExecutable command, Action<IDataParameterBinder> binder, Action<IDbExecutable, T> completion)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(completion != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var instance = command.CreateOnConnection(connection))
			{
				binder((IDataParameterBinder)instance);
				instance.ExecuteScalar<T>(completion);
			}
		}

		public static void ImmediateExecuteScalar<T>(this IDbExecutable command, string connection, Action<IDbExecutable, T> completion)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires(connection.Length > 0);
			Contract.Requires<ArgumentNullException>(completion != null);

			using (var instance = command.CreateOnConnection(connection))
			{
				instance.ExecuteScalar<T>(completion);
			}
		}

		public static IEnumerable<IDataRecord> ImmediateExecuteEnumerable(this IDbExecutable command, string connection)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires(connection.Length > 0);
			Contract.Ensures(Contract.Result<IEnumerable<IDataRecord>>() != null);

			using (var context = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				foreach (var record in instance.ExecuteEnumerable())
				{
					yield return record;
				}
			}
		}

		public static IEnumerable<IDataRecord> ExecuteEnumerable(this IDbExecutable command)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Ensures(Contract.Result<IEnumerable<IDataRecord>>() != null);

			foreach (var record in command.ExecuteEnumerable())
			{
				yield return record;
			}
		}
			
		public static T ExecuteSingle<T>(this IDbExecutable command, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);

			return ExecuteSingle<T>(command, null, transform);
		}

		public static T ExecuteSingle<T>(this IDbExecutable command, Action<IDataParameterBinder> bind, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);

			if (bind != null) bind(command.ParameterBinder);

			T result = default(T);
			command.ExecuteReader(res =>
			{
				var reader = res.Result;				
				if (!reader.Read())
				{
					throw new InvalidOperationException(Resources.Err_EmptyDbResult);
				}
				result = transform(reader);
				if (reader.Read())
				{
					throw new InvalidOperationException(Resources.Err_DuplicateDbResult);
				}
			});
			return result;
		}

		 public static T ExecuteSingleOrDefault<T>(this IDbExecutable command, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);

			return ExecuteSingleOrDefault<T>(command, null, transform);
		}

		 public static T ExecuteSingleOrDefault<T>(this IDbExecutable command, Action<IDataParameterBinder> bind, Func<IDataRecord, T> transform)
		 {
			 Contract.Requires<ArgumentNullException>(command != null);

			 if (bind != null) bind(command.ParameterBinder);
			 T result = default(T);
			 command.ExecuteReader(res =>
			 {
				 var reader = res.Result;
				 if (reader.Read())
				 {
					 result = transform(reader);
					 if (reader.Read())
					 {
						 throw new InvalidOperationException(Resources.Err_DuplicateDbResult);
					 }
				 }
			 });
			 return result;
		 }

		public static T ExecuteFirst<T>(this IDbExecutable command, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);

			return ExecuteFirst<T>(command, null, transform);
		}

		public static T ExecuteFirst<T>(this IDbExecutable command, Action<IDataParameterBinder> bind, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(transform != null);

			if (bind != null) bind(command.ParameterBinder);
			T result = default(T);
			command.ExecuteReader(res =>
			{
				var reader = res.Result;
				if (!reader.Read())
				{
					throw new InvalidOperationException(Resources.Err_EmptyDbResult);
				}
				result = transform(reader);				
			});
			return result;
		}

		public static T ExecuteFirstOrDefault<T>(this IDbExecutable command, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(transform != null);

			return ExecuteFirstOrDefault<T>(command, null, transform);
		}

		public static T ExecuteFirstOrDefault<T>(this IDbExecutable command, Action<IDataParameterBinder> bind, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(transform != null);

			if (bind != null) bind(command.ParameterBinder);
			T result = default(T);
			command.ExecuteReader(res =>
			{
				var reader = res.Result;
				if (reader.Read())
				{
					result = transform(reader);
				}
			});
			return result;
		}

		public static T ImmediateExecuteSingle<T>(this IDbExecutable command, string connection, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentException>(connection.Length > 0);
			Contract.Requires<ArgumentNullException>(transform != null);

			using (var instance = command.CreateOnConnection(connection))
			{
				return ExecuteSingle<T>(instance, null, transform);
			}
		}

		public static T ImmediateExecuteSingle<T>(this IDbExecutable command, string connection, Action<IDataParameterBinder> bind, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentException>(connection.Length > 0);
			Contract.Requires<ArgumentNullException>(transform != null);

			using (var instance = command.CreateOnConnection(connection))
			{
				return ExecuteSingle<T>(instance, bind, transform);
			}
		}

		public static T ImmediateExecuteSingle<T>(this IDbExecutable command, DbConnection connection, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(transform != null);
			
			using (var instance = command.CreateOnConnection(connection))
			{
				return ExecuteSingle<T>(instance, null, transform);
			}
		}

		public static T ImmediateExecuteSingle<T>(this IDbExecutable command, DbConnection connection, Action<IDataParameterBinder> bind, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(transform != null);
			
			using (var instance = command.CreateOnConnection(connection))
			{
				return ExecuteSingle<T>(instance, bind, transform);
			}
		}

		public static T ImmediateExecuteSingleOrDefault<T>(this IDbExecutable command, string connection, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentException>(connection.Length > 0);
			Contract.Requires<ArgumentNullException>(transform != null);

			using (var instance = command.CreateOnConnection(connection))
			{
				return ExecuteSingleOrDefault<T>(instance, null, transform);
			}
		}

		public static T ImmediateExecuteSingleOrDefault<T>(this IDbExecutable command, string connection, Action<IDataParameterBinder> bind, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentException>(connection.Length > 0);
			Contract.Requires<ArgumentNullException>(transform != null);

			using (var instance = command.CreateOnConnection(connection))
			{
				return ExecuteSingleOrDefault<T>(instance, bind, transform);
			}
		}

		public static T ImmediateExecuteSingleOrDefault<T>(this IDbExecutable command, DbConnection connection, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(transform != null);

			using (var instance = command.CreateOnConnection(connection))
			{
				return ExecuteSingleOrDefault<T>(instance, null, transform);
			}
		}

		public static T ImmediateExecuteSingleOrDefault<T>(this IDbExecutable command, DbConnection connection, Action<IDataParameterBinder> bind, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentNullException>(transform != null);

			using (var instance = command.CreateOnConnection(connection))
			{
				return ExecuteSingleOrDefault<T>(instance, bind, transform);
			}
		}

		public static IDbExecutable DefineParameter(this IDbExecutable command, string paramName, DbType dbType)
		{
			command.ParameterBinder.DefineParameter(paramName, dbType);
			return command;
		}

		[SuppressMessage("Microsoft.Design", "CA1006", Justification = "By design.")]
		public static IDbExecutable DefineParameter<T>(this IDbExecutable exe, Expression<Func<T, object>> expression)
		{
			Contract.Requires<ArgumentNullException>(exe != null);
			Contract.Requires<ArgumentNullException>(expression != null);
			Contract.Ensures(Contract.Result<IDbExecutable>() != null);

			return DefineParameter(exe, expression, ParameterDirection.Input);
		}

		[SuppressMessage("Microsoft.Design", "CA1006", Justification = "By design.")]
		public static IDbExecutable DefineParameter<T>(this IDbExecutable exe, Expression<Func<T, object>> expression, ParameterDirection direction)
		{
			Contract.Requires<ArgumentNullException>(exe != null);
			Contract.Requires<ArgumentNullException>(expression != null);
			Contract.Ensures(Contract.Result<IDbExecutable>() != null);

			MemberInfo member = expression.GetMemberFromExpression();
			Contract.Assert(member != null, "Expression must reference a field or property member");

			var memberType = member.MemberType;
			Contract.Assert(memberType == MemberTypes.Property, "Expression must reference a property member");

			exe.ParameterBinder.DefineParameter(member.Name, member.GetTypeOfValue(), direction);
			return exe;
		}

		public static IDbExecutable DefineParameter(this IDbExecutable exe, PropertyInfo property)
		{
			Contract.Requires<ArgumentNullException>(exe != null);
			Contract.Requires<ArgumentNullException>(property != null);
			Contract.Ensures(Contract.Result<IDbExecutable>() != null);

			return DefineParameter(exe, property, ParameterDirection.Input);
		}

		public static IDbExecutable DefineParameter(this IDbExecutable exe, PropertyInfo property, ParameterDirection direction)
		{
			Contract.Requires<ArgumentNullException>(exe != null);
			Contract.Requires<ArgumentNullException>(property != null);
			Contract.Ensures(Contract.Result<IDbExecutable>() != null);

			exe.ParameterBinder.DefineParameter(property.Name, property.PropertyType, direction);
			return exe;
		}

		public static IDbExecutable DefineParameter(this IDbExecutable exe, string bindName, PropertyInfo property, ParameterDirection direction)
		{
			Contract.Requires<ArgumentNullException>(exe != null);
			Contract.Requires<ArgumentNullException>(bindName != null);
			Contract.Requires<ArgumentException>(bindName.Length > 0);
			Contract.Requires<ArgumentNullException>(property != null);
			Contract.Requires<ArgumentNullException>(property.Name != null);
			Contract.Requires<ArgumentNullException>(property.Name.Length > 0);
			Contract.Ensures(Contract.Result<IDbExecutable>() != null);

			exe.ParameterBinder.DefineParameter(() => new DbParamDefinition(property.Name, bindName, property.PropertyType, direction));
			return exe;
		}
	}
}
