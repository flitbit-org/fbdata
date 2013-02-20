using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using FlitBit.Core;
using FlitBit.Emit;
using FlitBit.Core.Parallel;

namespace FlitBit.Data
{
	public static class IDbExecutableExtensions
	{
		public static void ExecuteNonQuery(this IDbExecutable exe, IDbContext context, Action<IDbExecutable, int> after)
		{
			Contract.Requires<ArgumentNullException>(exe != null);
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(after != null);

			after(exe, exe.ExecuteNonQuery(context));
		}

		public static void ExecuteReader(this IDbExecutable exe, IDbContext context, Action<IDbExecutable, IDataReader> after)
		{
			Contract.Requires<ArgumentNullException>(exe != null);
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(after != null);

			after(exe, exe.ExecuteReader(context));
		}

		public static void ExecuteScalar<T>(this IDbExecutable exe, IDbContext context, Action<IDbExecutable, T> after)
		{
			Contract.Requires<ArgumentNullException>(exe != null);
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(after != null);

			after(exe, exe.ExecuteScalar<T>(context));
		}

		public static Completion<DbResult<int>> ExecuteNonQueryWithCompletion(this IDbExecutable exe, IDbContext context, Continuation<DbResult<int>> continuation)
		{
			Contract.Requires<ArgumentNullException>(exe != null);
			Contract.Requires<ArgumentNullException>(context != null);

			var result = new Completion<DbResult<int>>(exe);
			exe.ExecuteNonQuery(context, (e, res) =>
			{
				if (e != null) result.MarkFaulted(e);
				else result.MarkCompleted(res);
			});
			if (continuation != null)	result.Continue(continuation);
			return result;
		}

		public static Completion<DbResult<IDataReader>> ExecuteReaderWithCompletion(this IDbExecutable exe, IDbContext context, Continuation<DbResult<IDataReader>> continuation)
		{
			Contract.Requires<ArgumentNullException>(exe != null);
			Contract.Requires<ArgumentNullException>(context != null);

			var result = new Completion<DbResult<IDataReader>>(exe);
			exe.ExecuteReader(context, (e, res) =>
			{
				if (e != null) result.MarkFaulted(e);
				else result.MarkCompleted(res);
			});
			if (continuation != null) result.Continue(continuation);
			return result;
		}

		public static Completion<DbResult<T>> ExecuteScalarWithCompletion<T>(this IDbExecutable exe, IDbContext context, Continuation<DbResult<T>> continuation)
		{
			Contract.Requires<ArgumentNullException>(exe != null);
			Contract.Requires<ArgumentNullException>(context != null);

			var result = new Completion<DbResult<T>>(exe);
			exe.ExecuteScalar<T>(context, (e, res) =>
			{
				if (e != null) result.MarkFaulted(e);
				else result.MarkCompleted(res);
			});
			if (continuation != null) result.Continue(continuation);
			return result;
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

			return exe.DefineParameter(member.Name, member.GetTypeOfValue(), direction);
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

			return exe.DefineParameter(property.Name, property.PropertyType, direction);
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

			return exe.DefineParameter(() => new DbParamDefinition(property.Name, bindName, property.PropertyType, direction));
		}
	}
}
