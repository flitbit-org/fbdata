#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion


using System;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace FlitBit.Data
{
	public partial class BasicDbExecutable : IDataParameterBinder
	{
		public bool ContainsParameter(string name)
		{
			return IndexOfParameter(name) >= 0;
		}
		public virtual string PrepareParameterName(string name)
		{
			return name;
		}
		public virtual DbTypeTranslation TranslateRuntimeType(Type type)
		{
			return DbTypeTranslations.TranslateRuntimeType(type);
		}
		IDataParameterBinder IDataParameterBinder.DefineParameter<T>(Expression<Func<T, object>> expression)
		{
			return (IDataParameterBinder)DefineParameter(expression, ParameterDirection.Input);
		}
		IDataParameterBinder IDataParameterBinder.DefineParameter<T>(Expression<Func<T, object>> expression, ParameterDirection direction)
		{
			return (IDataParameterBinder)DefineParameter(expression, direction);
		}
		IDataParameterBinder IDataParameterBinder.DefineParameter(PropertyInfo property)
		{
			return (IDataParameterBinder)DefineParameter(property, ParameterDirection.Input);
		}
		IDataParameterBinder IDataParameterBinder.DefineParameter(PropertyInfo property, ParameterDirection direction)
		{
			return (IDataParameterBinder)DefineParameter(property, direction);
		}
		IDataParameterBinder IDataParameterBinder.DefineParameter(string bindName, PropertyInfo property, ParameterDirection direction)
		{
			return (IDataParameterBinder)DefineParameter(bindName, property, direction);
		}
		IDataParameterBinder IDataParameterBinder.DefineParameter(string name, Type runtimeType)
		{
			return (IDataParameterBinder)DefineParameter(name, runtimeType, ParameterDirection.Input);
		}
		IDataParameterBinder IDataParameterBinder.DefineParameter(string name, Type runtimeType, ParameterDirection direction)
		{
			return (IDataParameterBinder)DefineParameter(name, runtimeType, ParameterDirection.Input);
		}
		IDataParameterBinder IDataParameterBinder.DefineParameter(string name, Type runtimeType, int length, ParameterDirection direction)
		{
			return (IDataParameterBinder)DefineParameter(name, runtimeType, length, ParameterDirection.Input);
		}		
		IDataParameterBinder IDataParameterBinder.DefineParameter(string name, DbType dbType)
		{
			return (IDataParameterBinder)DefineParameter(name, dbType, ParameterDirection.Input);
		}
		IDataParameterBinder IDataParameterBinder.DefineParameter(string name, DbType dbType, ParameterDirection direction)
		{
			return (IDataParameterBinder)DefineParameter(name, dbType, direction);

		}
		IDataParameterBinder IDataParameterBinder.DefineParameter(string name, DbType dbType, int length)
		{
			return (IDataParameterBinder)DefineParameter(name, dbType, length, ParameterDirection.Input);

		}
		IDataParameterBinder IDataParameterBinder.DefineParameter(string name, DbType dbType, int length, ParameterDirection direction)
		{
			return (IDataParameterBinder)DefineParameter(name, dbType, length, direction);

		}
		IDataParameterBinder IDataParameterBinder.DefineParameter(string name, DbType dbType, int size, byte scale)
		{
			return (IDataParameterBinder)DefineParameter(name, dbType, size, scale, ParameterDirection.Input);
		}
		IDataParameterBinder IDataParameterBinder.DefineParameter(string name, DbType dbType, int size, byte scale, ParameterDirection direction)
		{
			return (IDataParameterBinder)DefineParameter(name, dbType, size, scale, direction);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, bool value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, byte[] value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, byte value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, DateTime value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, decimal value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, double value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, Guid value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, float value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, sbyte value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, string value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, short value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, int value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, long value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, ushort value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, uint value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue(string name, ulong value)
		{
			return (IDataParameterBinder)SetParameterValue(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValue<T>(string name, T value)
		{
			return (IDataParameterBinder)SetParameterValue<T>(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterValueAsEnum<E>(string name, E value)
		{
			return (IDataParameterBinder)SetParameterValueAsEnum<E>(name, value);
		}
		IDataParameterBinder IDataParameterBinder.SetParameterDbNull(string name)
		{
			return (IDataParameterBinder)SetParameterDbNull(name);
		}
	}
}
