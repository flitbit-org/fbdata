#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion


using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using FlitBit.Core.Properties;

namespace FlitBit.Data
{
	
	public static partial class Extensions
	{
		public static int ImmediateExecuteNonQuery(this IDbExecutable command)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			
			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				return instance.ExecuteNonQuery(create);
			}
		}		
		public static int ImmediateExecuteNonQuery(this IDbExecutable command, Action<IDbExecutable, int> completion)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(completion != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				return instance.ExecuteNonQuery(create, completion);
			}
		}
		public static int ImmediateExecuteNonQuery(this IDbExecutable command, Action<IDataParameterBinder> binder)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(binder != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				binder((IDataParameterBinder)instance);
				return instance.ExecuteNonQuery(create);
			}
		}
		public static void ImmediateExecute(this IDbExecutable command, Action<IDataParameterBinder> binder, Action<IDbContext, IDataReader> handler)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Assert(handler != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				binder((IDataParameterBinder)instance);
				instance.ExecuteReader(create, handler);
			}
		}
		public static void ImmediateExecute(this IDbExecutable command, Action<IDbContext, IDataReader> handler)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Assert(handler != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				instance.ExecuteReader(create, handler);
			}
		}
		public static int ImmediateExecuteNonQuery(this IDbExecutable command, Action<IDataParameterBinder> binder, Action<IDbExecutable, int> completion)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(completion != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				binder((IDataParameterBinder)instance);
				return instance.ExecuteNonQuery(create, completion);
			}
		}
		public static int ImmediateExecuteNonQuery(this IDbExecutable command, string connection, Action<IDbExecutable, int> completion)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires(connection.Length > 0);
			Contract.Requires<ArgumentNullException>(completion != null);

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				return instance.ExecuteNonQuery(create, completion);
			}
		}

		public static T ImmediateExecuteScalar<T>(this IDbExecutable command, Action<IDbExecutable, T> completion)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(completion != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				return instance.ExecuteScalar<T>(create, completion);
			}
		}
		public static T ImmediateExecuteScalar<T>(this IDbExecutable command, Action<IDataParameterBinder> binder)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(binder != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				binder((IDataParameterBinder)instance);
				return instance.ExecuteScalar<T>(create);
			}
		}
		public static T ImmediateExecuteScalar<T>(this IDbExecutable command, Action<IDataParameterBinder> binder, Action<IDbExecutable, T> completion)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(completion != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				binder((IDataParameterBinder)instance);
				return instance.ExecuteScalar<T>(create, completion);
			}
		}
		public static T ImmediateExecuteScalar<T>(this IDbExecutable command, string connection, Action<IDbExecutable, T> completion)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires(connection.Length > 0);
			Contract.Requires<ArgumentNullException>(completion != null);

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				return instance.ExecuteScalar<T>(create, completion);
			}
		}

		public static IEnumerable<T> ImmediateExecuteTransformAll<T>(this IDbExecutable command, string connection, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(transform != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires(connection.Length > 0);

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				return instance.ExecuteTransformAll(create, transform);
			}
		}
		public static IEnumerable<T> ImmediateExecuteTransformAll<T>(this IDbExecutable command, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				return instance.ExecuteTransformAll(create, transform);
			}
		}

		public static IEnumerable<T> ImmediateExecuteTransformAll<T>(this IDbExecutable command, string connection, Action<IDataParameterBinder> binder, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(transform != null);
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires(connection.Length > 0);
			Contract.Requires<ArgumentNullException>(binder != null);

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				binder((IDataParameterBinder)instance);
				return instance.ExecuteTransformAll(create, transform);
			}
		}
		public static IEnumerable<T> ImmediateExecuteTransformAll<T>(this IDbExecutable command, Action<IDataParameterBinder> binder, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(binder != null);
			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				binder((IDataParameterBinder)instance);

				return instance.ExecuteTransformAll(create, transform);
			}
		}

		public static T ImmediateExecuteTransformSingleOrDefault<T>(this IDbExecutable command, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				return instance.ExecuteTransformSingleOrDefault(create, transform);
			}
		}
		public static T ImmediateExecuteTransformSingleOrDefault<T>(this IDbExecutable command, Action<IDataParameterBinder> binder, Func<IDataRecord, T> transform)
		{
			Contract.Requires<ArgumentNullException>(command != null);

			var connection = command.ConnectionName;
			if (String.IsNullOrEmpty(connection))
				throw new InvalidOperationException("Cannot to execute a command definition without a connection.");

			using (var create = DbContext.SharedOrNewContext())
			using (var instance = command.CreateOnConnection(connection))
			{
				binder((IDataParameterBinder)instance);
				return instance.ExecuteTransformSingleOrDefault(create, transform);
			}
		}
	}
}
