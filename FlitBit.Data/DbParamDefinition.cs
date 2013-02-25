#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Data;
using System.Diagnostics.Contracts;
using FlitBit.Core.Properties;
using FlitBit.Core;

namespace FlitBit.Data
{
	public struct DbParamDefinition
	{
		static readonly int CHashCodeSeed = typeof(DbParamDefinition).AssemblyQualifiedName.GetHashCode();
		public static readonly DbType CInvalidDbType = (DbType)0x7FFF;

		string _name;
		string _bindName;
		ParameterDirection _direction;
		DbType _dbType;
		int _specializedDbType;
		int _size;
		byte _scale;
		Type _runtimeType;
		
		public DbParamDefinition(string name, string bindName, DbType dbType, ParameterDirection direction)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentException>(name.Length > 0);
			this._name = name;
			this._bindName = bindName;
			this._direction = direction;
			this._dbType = dbType;
			this._specializedDbType = (int)CInvalidDbType;
			this._size = 0;
			this._scale = 0;
			this._runtimeType = null;
		}
		public DbParamDefinition(string name, string bindName, DbType dbType, int length, ParameterDirection direction)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires(name.Length > 0);
			this._name = name;
			this._bindName = bindName;
			this._direction = direction;
			this._dbType = dbType;
			this._specializedDbType = (int)CInvalidDbType;
			this._size = length;
			this._scale = 0;
			this._runtimeType = null;
		}
		public DbParamDefinition(string name, string bindName, DbType dbType, int size, byte scale, ParameterDirection direction)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires(name.Length > 0);

			this._name = name;
			this._bindName = bindName;
			this._direction = direction;
			this._dbType = dbType;
			this._specializedDbType = (int)CInvalidDbType;
			this._size = size;
			this._scale = scale;
			this._runtimeType = null;
		}
		public DbParamDefinition(string name, string bindName, DbType dbType, int specializedDbType, int size, byte scale, ParameterDirection direction)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires(name.Length > 0);
			Contract.Requires<ArgumentNullException>(bindName != null);
			Contract.Requires(bindName.Length > 0);

			this._name = name;
			this._bindName = bindName;
			this._direction = direction;
			this._dbType = dbType;
			this._specializedDbType = specializedDbType;
			this._size = size;
			this._scale = scale;
			this._runtimeType = null;
		}		
		public DbParamDefinition(string name, string bindName, Type type, ParameterDirection direction)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires(name.Length > 0);
			Contract.Requires<ArgumentNullException>(type != null);

			this._name = name;
			this._bindName = bindName;
			this._direction = direction;
			this._dbType = CInvalidDbType;
			this._specializedDbType = (int)CInvalidDbType;			
			this._size = 0;
			this._scale = 0;
			this._runtimeType = type;
		}
		public DbParamDefinition(string name, string bindName, Type type, int length, ParameterDirection direction)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires(name.Length > 0);
			Contract.Requires<ArgumentNullException>(type != null);

			this._name = name;
			this._bindName = bindName;
			this._direction = direction;
			this._dbType = CInvalidDbType;
			this._specializedDbType = (int)CInvalidDbType;
			this._size = length;
			this._scale = 0;
			this._runtimeType = type;
		}

		public string Name { get { return _name; } }
		public string BindName { get { return _bindName; } }
		public ParameterDirection Direction { get { return _direction; } }
		public DbType DbType { get { return _dbType; } }
		public int SpecializedDbType { get { return _specializedDbType; } }
		public int Size { get { return _size; } }
		public byte Scale { get { return _scale; } }
		public Type RuntimeType { get { return _runtimeType; } }

		#region Object overrides

		public override string ToString()
		{
			return String.Concat("DbParamDefinition { Name: '", _name, 				
				"', BindName: '", _bindName,
				"', DbType: ", _dbType,
				", SpecializedDbType: ", _specializedDbType,
				", Size: ", _size,
				", Scale: ", _scale,
				", Direction: ", _direction,
				", RuntimeType: '", _runtimeType.Name,
				" }");
		}

		public bool Equals(DbParamDefinition other)
		{
			return String.Equals(this._name, other._name)
				&& String.Equals(this._bindName, other._bindName)
				&& this._dbType == other._dbType
				&& this._direction == other._direction
				&& Object.Equals(this._runtimeType, other._runtimeType)
				&& this._scale == other._scale
				&& this._size == other._size
				&& this._specializedDbType == other._specializedDbType;
		}

		public override bool Equals(object obj)
		{
			return typeof(DbParamDefinition).IsInstanceOfType(obj)
				&& this.Equals((DbParamDefinition)obj);
		}

		public override int GetHashCode()
		{
			int prime = Constants.NotSoRandomPrime; // a random prime
			int code = CHashCodeSeed * prime;
			if (_name != null)
				code ^= _name.GetHashCode() * prime;
			if (_bindName != null)
				code ^= _bindName.GetHashCode() * prime;

			code ^= (int)_direction * prime;
			code ^= (int)_dbType * prime;
			code ^= _specializedDbType * prime;
			code ^= _size * prime;
			code ^= _scale * prime;
			if (_runtimeType != null)
				code ^= _runtimeType.AssemblyQualifiedName.GetHashCode() * prime;
			return code;
		}

		public static bool operator ==(DbParamDefinition lhs, DbParamDefinition rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(DbParamDefinition lhs, DbParamDefinition rhs)
		{
			return !lhs.Equals(rhs);
		}

		#endregion
	}
}
