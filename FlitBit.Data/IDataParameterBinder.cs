#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion


using System;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics.Contracts;
using FlitBit.Core.Properties;
using System.Diagnostics.CodeAnalysis;

namespace FlitBit.Data
{
	
	public interface IDataParameterBinder
	{
		bool ContainsParameter(string name);
		string PrepareParameterName(string name);
		DbTypeTranslation TranslateRuntimeType(Type type);

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		[SuppressMessage("Microsoft.Design", "CA1006", Justification = "By design.")]
		IDataParameterBinder DefineParameter<T>(Expression<Func<T, object>> expression);
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		[SuppressMessage("Microsoft.Design", "CA1006", Justification = "By design.")]
		IDataParameterBinder DefineParameter<T>(Expression<Func<T, object>> expression, ParameterDirection direction);
		IDataParameterBinder DefineParameter(PropertyInfo prop);
		IDataParameterBinder DefineParameter(PropertyInfo prop, ParameterDirection direction);
		IDataParameterBinder DefineParameter(string bindName, PropertyInfo prop, ParameterDirection direction);

		IDataParameterBinder DefineParameter(string name, Type runtimeType);
		IDataParameterBinder DefineParameter(string name, Type runtimeType, ParameterDirection direction);
		IDataParameterBinder DefineParameter(string name, Type runtimeType, int length, ParameterDirection direction);
		IDataParameterBinder DefineParameter(string name, DbType dbType);
		IDataParameterBinder DefineParameter(string name, DbType dbType, ParameterDirection direction);
		IDataParameterBinder DefineParameter(string name, DbType dbType, int length);
		IDataParameterBinder DefineParameter(string name, DbType dbType, int length, ParameterDirection direction);
		IDataParameterBinder DefineParameter(string name, DbType dbType, int size, byte scale);
		IDataParameterBinder DefineParameter(string name, DbType dbType, int size, byte scale, ParameterDirection direction);

		IDataParameterBinder SetParameterValue(string name, bool value);
		IDataParameterBinder SetParameterValue(string name, byte[] value);
		IDataParameterBinder SetParameterValue(string name, byte value);
		IDataParameterBinder SetParameterValue(string name, DateTime value);
		IDataParameterBinder SetParameterValue(string name, decimal value);
		IDataParameterBinder SetParameterValue(string name, Double value);
		IDataParameterBinder SetParameterValue(string name, Guid value);
		IDataParameterBinder SetParameterValue(string name, Single value);
		
		IDataParameterBinder SetParameterValue(string name, SByte value);
		IDataParameterBinder SetParameterValue(string name, string value);
		IDataParameterBinder SetParameterValue(string name, Int16 value);
		IDataParameterBinder SetParameterValue(string name, Int32 value);
		IDataParameterBinder SetParameterValue(string name, Int64 value);
		
		IDataParameterBinder SetParameterValue(string name, UInt16 value);
		
		IDataParameterBinder SetParameterValue(string name, UInt32 value);
		
		IDataParameterBinder SetParameterValue(string name, UInt64 value);
		IDataParameterBinder SetParameterValue<T>(string name, T value);
		IDataParameterBinder SetParameterValueAsEnum<E>(string name, E value);

		IDataParameterBinder SetParameterDbNull(string name);
	}

	public static partial class Extensions
	{
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, bool value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(bool));
			}
			binder.SetParameterValue(name, value);			
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, byte value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(byte));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, int length, byte[] value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(byte[]), length, ParameterDirection.Input);
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, byte[] value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(byte[]));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, DateTime value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(DateTime));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, decimal value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(decimal));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, double value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(double));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, Guid value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(Guid));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, float value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(float));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, sbyte value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(sbyte));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, int length, string value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(string), length, ParameterDirection.Input);
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, string value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(string));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, short value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(short));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, int value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(int));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, long value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(long));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, ushort value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(ushort));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, uint value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(uint));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name, ulong value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(ulong));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		
		public static IDataParameterBinder DefineAndBindParameter<T>(this IDataParameterBinder binder, string name, T value)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(T));
			}
			binder.SetParameterValue(name, value);
			return binder;
		}
		
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		public static IDataParameterBinder DefineAndSetDbNull<T>(this IDataParameterBinder binder, string name)
		{
			Contract.Requires<ArgumentNullException>(binder != null);
			Contract.Requires<ArgumentNullException>(name != null);

			if (!binder.ContainsParameter(name))
			{
				binder.DefineParameter(name, typeof(T));
			}
			binder.SetParameterDbNull(name);
			return binder;
		}
	}
}
