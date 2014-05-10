#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics.Contracts;

namespace FlitBit.Data.SqlServer
{
	public class DirectSqlParameterBinder : IDataParameterBinder
	{
		SqlCommand _command;
		SqlParameter _mru;

		public DirectSqlParameterBinder(SqlCommand cmd)
		{
			Contract.Requires<ArgumentNullException>(cmd != null);
			_command = cmd;
		}

		#region IDataParameterBinder Members

		public string PrepareParameterName(string name)
		{
			Contract.Assert(name != null);
			Contract.Assert(name.Length > 0);
			return (name[0] != '@') ? String.Concat('@', name) : name;
		}

		public DbTypeTranslation TranslateRuntimeType(Type type)
		{
			return SqlDbTypeTranslations.TranslateRuntimeType(type);
		}

		public bool ContainsParameter(string name)
		{
			var bindName = PrepareParameterName(name);
			return _command.Parameters.Contains(name);
		}

		public int IndexOfParameter(string name)
		{
			var bindName = PrepareParameterName(name);
			return _command.Parameters.IndexOf(bindName);
		}

		public IEnumerable<ParameterBinding> Bindings { get { throw new NotImplementedException(); } }

		public IDataParameterBinder DefineParameter(string name, Type runtimeType)
		{
			var trans = SqlDbTypeTranslations.TranslateRuntimeType(runtimeType);
			_command.Parameters.Add(new SqlParameter(PrepareParameterName(name), (SqlDbType) trans.SpecializedDbType));
			return this;
		}

		public IDataParameterBinder DefineParameter(string name, Type runtimeType, ParameterDirection direction)
		{
			var trans = SqlDbTypeTranslations.TranslateRuntimeType(runtimeType);
			_mru = new SqlParameter(PrepareParameterName(name), (SqlDbType) trans.SpecializedDbType);
			_mru.Direction = direction;
			_command.Parameters.Add(_mru);
			return this;
		}

		public IDataParameterBinder DefineParameter(string name, Type runtimeType, int length, ParameterDirection direction)
		{
			var trans = SqlDbTypeTranslations.TranslateRuntimeType(runtimeType);
			_mru = new SqlParameter(PrepareParameterName(name), (SqlDbType) trans.SpecializedDbType);
			_mru.Size = length;
			_mru.Direction = direction;
			_command.Parameters.Add(_mru);
			return this;
		}

		public IDataParameterBinder DefineParameter(string name, DbType dbType)
		{
			var trans = SqlDbTypeTranslations.TranslateDbType(dbType);
			_mru = new SqlParameter(PrepareParameterName(name), (SqlDbType) trans.SpecializedDbType);
			_command.Parameters.Add(_mru);
			return this;
		}

		public IDataParameterBinder DefineParameter(string name, DbType dbType, ParameterDirection direction)
		{
			var trans = SqlDbTypeTranslations.TranslateDbType(dbType);
			_mru = new SqlParameter(PrepareParameterName(name), (SqlDbType) trans.SpecializedDbType);
			_mru.Direction = direction;
			_command.Parameters.Add(_mru);
			return this;
		}

		public IDataParameterBinder DefineParameter(string name, DbType dbType, int length)
		{
			var trans = SqlDbTypeTranslations.TranslateDbType(dbType);
			_mru = new SqlParameter(PrepareParameterName(name), (SqlDbType) trans.SpecializedDbType);
			_command.Parameters.Add(_mru);
			return this;
		}

		public IDataParameterBinder DefineParameter(string name, DbType dbType, int length, ParameterDirection direction)
		{
			var trans = SqlDbTypeTranslations.TranslateDbType(dbType);
			_mru = new SqlParameter(PrepareParameterName(name), (SqlDbType) trans.SpecializedDbType);
			_mru.Direction = direction;
			_mru.Size = length;
			_command.Parameters.Add(_mru);
			return this;
		}

		public IDataParameterBinder DefineParameter(string name, DbType dbType, int size, byte scale)
		{
			var trans = SqlDbTypeTranslations.TranslateDbType(dbType);
			_mru = new SqlParameter(PrepareParameterName(name), (SqlDbType) trans.SpecializedDbType);
			_mru.Precision = (byte) size;
			_mru.Scale = scale;
			_command.Parameters.Add(_mru);
			return this;
		}

		public IDataParameterBinder DefineParameter(string name, DbType dbType, int size, byte scale,
			ParameterDirection direction)
		{
			var trans = SqlDbTypeTranslations.TranslateDbType(dbType);
			_mru = new SqlParameter(PrepareParameterName(name), (SqlDbType) trans.SpecializedDbType);
			_mru.Direction = direction;
			_mru.Precision = (byte) size;
			_mru.Scale = scale;
			_command.Parameters.Add(_mru);
			return this;
		}

		public IDataParameterBinder DefineParameter(Func<DbParamDefinition> specializeParam)
		{
			throw new NotImplementedException();
		}

		public IDataParameterBinder SetParameterValue(string name, bool value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlBoolean(value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, byte[] value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlBinary(value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, byte value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlByte(value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, DateTime value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlDateTime(value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, decimal value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlDecimal(value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, double value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlDouble(value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, Guid value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlGuid(value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, float value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlSingle(value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, sbyte value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlByte((byte) value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, string value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlString(value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, short value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlInt16(value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, int value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlInt32(value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, long value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlInt64(value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, ushort value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlInt16((short) value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, uint value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlInt32((int) value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, ulong value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = new SqlInt64((long) value);
			return this;
		}

		public IDataParameterBinder SetParameterValue<T>(string name, T value)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			return this;
		}

		public IDataParameterBinder SetParameterValueAsEnum<E>(string name, E value, bool useName)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}
			if (useName)
			{
				_mru.SqlValue = new SqlString(Enum.GetName(typeof(E), value));
			}
			else
			{
				switch (Type.GetTypeCode(Enum.GetUnderlyingType(typeof(E))))
				{
					case TypeCode.SByte:
						_mru.SqlValue = new SqlByte(Convert.ToByte(value));
						break;
					case TypeCode.Int16:
						_mru.SqlValue = new SqlInt16(Convert.ToInt16(value));
						break;
					case TypeCode.Int32:
						_mru.SqlValue = new SqlInt32(Convert.ToInt32(value));
						break;
					case TypeCode.Int64:
						_mru.SqlValue = new SqlInt64(Convert.ToInt64(value));
						break;
				}
			}
			return this;
		}

		public IDataParameterBinder SetParameterDbNull(string name)
		{
			var bindName = PrepareParameterName(name);
			if (_mru == null || !String.Equals(bindName, _mru.ParameterName))
			{
				_mru = _command.Parameters[bindName];
			}

			_mru.SqlValue = DBNull.Value;
			return this;
		}

		public bool PrepareDbCommand(DbCommand command)
		{
			throw new NotImplementedException();
		}

		public void Initialize(IEnumerable<ParameterBinding> bindings)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}