#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Core;
using System.Data.Common;

namespace FlitBit.Data
{

	public partial class DataParameterBinder : IDataParameterBinder
	{
		ParameterBinding[] _parameters = new ParameterBinding[0];

		/// <summary>
		/// Gets the command's parameter definitions.
		/// </summary>
		public IEnumerable<ParameterBinding> Bindings { get { return _parameters.ToReadOnly(); } }

		internal void AddParameterBinding(DbParamDefinition def)
		{
			Contract.Assert(IndexOfParameter(def.Name) < 0, "Parameter already exists; parameter bind names must be unique.");
			ParameterBinding binding = new ParameterBinding();
			binding.Definition = def;
			_parameters = _parameters.Concat(new ParameterBinding[] { binding }).ToArray();
		}

		internal protected virtual IEnumerable<ParameterBinding> PrepareParametersFromSource(IEnumerable<ParameterBinding> parameters)
		{
			return parameters;
		}

		public bool PrepareDbCommand(DbCommand command)
		{
			Contract.Requires<ArgumentNullException>(command != null);

			var result = false;
			var parms = PrepareParametersFromSource(_parameters);
			foreach (var binding in parms)
			{
				var def = binding.Definition;
				// TODO: verify this works for BLOBs?
				if (def.Size > 0 || def.Size == -1)
				{
					if (def.Scale > 0)
					{
						MakeParameter(command, def.BindName, def.DbType, binding.Definition.SpecializedDbType, def.Size, def.Scale, def.Direction)
							.Value = binding.SpecializedValue;
					}
					else
					{
						MakeParameter(command, def.BindName, def.DbType, binding.Definition.SpecializedDbType, def.Size, def.Direction)
							.Value = binding.SpecializedValue;
					}
				}
				else
				{
					MakeParameter(command, def.BindName, def.DbType, binding.Definition.SpecializedDbType, def.Direction)
						.Value = binding.SpecializedValue;
				}
				if (!result && def.Direction.HasFlag(ParameterDirection.Output))
				{
					result = true;
				}
			}
			return result;
		}

		public virtual string PrepareParameterName(string name)
		{
			return name;
		}
		public virtual DbTypeTranslation TranslateRuntimeType(Type type)
		{
			return DbTypeTranslations.TranslateRuntimeType(type);
		}

		protected virtual string DetermineBindNameForParameter(DbParamDefinition definition)
		{
			return definition.Name;
		}

		protected virtual IDataParameter MakeParameter(DbCommand command, string name, DbType dbType, int specializedDbType, int length, ParameterDirection direction)
		{
			var result = command.CreateParameter();
			result.ParameterName = name;
			result.DbType = dbType;
			result.Direction = direction;
			return result;
		}
		protected virtual IDataParameter MakeParameter(DbCommand command, string name, DbType dbType, int specializedDbType, int size, byte scale, ParameterDirection direction)
		{
			var result = command.CreateParameter();
			result.ParameterName = name;
			result.DbType = dbType;
			result.Direction = direction;
			return result;
		}
		protected virtual IDataParameter MakeParameter(DbCommand command, string name, DbType dbType, int specializedDbType, ParameterDirection direction)
		{
			var result = command.CreateParameter();
			result.ParameterName = name;
			result.DbType = dbType;
			result.Direction = direction;
			return result;
		}

		public int IndexOfParameter(string name)
		{
			string preparedName = PrepareParameterName(name);
			int p = -1;
			for (int i = 0; i < _parameters.Length; i++)
			{
				if (String.Equals(_parameters[i].Definition.Name, name)
					|| String.Equals(_parameters[i].Definition.BindName, name)
					|| String.Equals(_parameters[i].Definition.BindName, preparedName))
				{
					p = i;
					break;
				}
			}
			return p;
		}

		public bool ContainsParameter(string name)
		{
			return IndexOfParameter(name) >= 0;
		}

		public IDataParameterBinder DefineParameter(string name, Type runtimeType)
		{
			return DefineParameter(name, runtimeType, ParameterDirection.Input);
		}

		public IDataParameterBinder DefineParameter(string name, Type runtimeType, ParameterDirection direction)
		{
			var bindName = PrepareParameterName(name);
			AddParameterBinding(new DbParamDefinition(name, bindName, runtimeType, direction));
			return this;
		}

		public IDataParameterBinder DefineParameter(string name, Type runtimeType, int length, ParameterDirection direction)
		{
			var bindName = PrepareParameterName(name);
			AddParameterBinding(new DbParamDefinition(name, bindName, runtimeType, length, direction));
			return this;
		}

		public IDataParameterBinder DefineParameter(string name, DbType dbType)
		{
			return DefineParameter(name, dbType, ParameterDirection.Input);
		}

		public IDataParameterBinder DefineParameter(string name, DbType dbType, ParameterDirection direction)
		{
			Contract.Assert(name != null);
			Contract.Assert(name.Length > 0);

			var bindName = PrepareParameterName(name);

			AddParameterBinding(new DbParamDefinition(name, bindName, dbType, direction));
			return this;
		}

		public IDataParameterBinder DefineParameter(string name, DbType dbType, int length)
		{
			return DefineParameter(name, dbType, length, ParameterDirection.Input);
		}

		public IDataParameterBinder DefineParameter(string name, DbType dbType, int length, ParameterDirection direction)
		{
			Contract.Assert(name != null);
			Contract.Assert(name.Length > 0);

			var bindName = PrepareParameterName(name);
			AddParameterBinding(new DbParamDefinition(name, bindName, dbType, length, direction));
			return this;
		}

		public IDataParameterBinder DefineParameter(string name, DbType dbType, int size, byte scale)
		{
			return DefineParameter(name, dbType, size, scale, ParameterDirection.Input);
		}

		public IDataParameterBinder DefineParameter(string name, DbType dbType, int size, byte scale, ParameterDirection direction)
		{
			Contract.Assert(name != null);
			Contract.Assert(name.Length > 0);

			var bindName = PrepareParameterName(name);
			AddParameterBinding(new DbParamDefinition(name, bindName, dbType, size, scale, direction));
			return this;
		}

		public IDataParameterBinder DefineParameter(Func<DbParamDefinition> specializeParam)
		{
			var param = specializeParam();
			AddParameterBinding(param);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, bool value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, byte[] value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, byte value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, DateTime value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, decimal value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, Double value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, Guid value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, Single value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, SByte value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, string value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, Int16 value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, Int32 value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, Int64 value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, UInt16 value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, UInt32 value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValue(string name, UInt64 value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}


		public IDataParameterBinder SetParameterValue<T>(string name, T value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValueT(_parameters, p, name, value);
			return this;
		}

		public IDataParameterBinder SetParameterValueAsEnum<E>(string name, E value, bool useName)
		{
			Contract.Assert(name != null);
			Contract.Assert(typeof(E).IsEnum, "typeof value must be an enum");
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValueAsEnum(_parameters, p, name, value, useName);
			return this;
		}


		public IDataParameterBinder SetParameterDbNull(string name)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterDbNull(_parameters, p, name);
			return this;
		}

		protected virtual void PerformSetParameterDbNull(ParameterBinding[] parameters, int p, string name)
		{
			parameters[p].SpecializedValue = DBNull.Value;
		}
		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, byte value)
		{
			parameters[p].SpecializedValue = value;
		}
		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, bool value)
		{
			parameters[p].SpecializedValue = value;
		}
		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, byte[] value)
		{
			parameters[p].SpecializedValue = value;
		}
		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, DateTime value)
		{
			parameters[p].SpecializedValue = value;
		}
		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, decimal value)
		{
			parameters[p].SpecializedValue = value;
		}
		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, double value)
		{
			parameters[p].SpecializedValue = value;
		}
		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, Guid value)
		{
			parameters[p].SpecializedValue = value;
		}
		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, float value)
		{
			parameters[p].SpecializedValue = value;
		}

		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, sbyte value)
		{
			parameters[p].SpecializedValue = value;
		}
		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, short value)
		{
			parameters[p].SpecializedValue = value;
		}
		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, int value)
		{
			parameters[p].SpecializedValue = value;
		}
		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, long value)
		{
			parameters[p].SpecializedValue = value;
		}
		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, string value)
		{
			parameters[p].SpecializedValue = value;
		}

		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, ushort value)
		{
			parameters[p].SpecializedValue = value;
		}

		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, uint value)
		{
			parameters[p].SpecializedValue = value;
		}

		protected virtual void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, ulong value)
		{
			parameters[p].SpecializedValue = value;
		}

		protected virtual void PerformSetParameterValueAsEnum<E>(ParameterBinding[] parameters, int p, string name, E value, bool useName)
		{
			if (useName)
			{
				_parameters[p].SpecializedValue = Enum.GetName(typeof(E), value);
			}
			else
			{
				_parameters[p].SpecializedValue = Convert.ToInt32(value);
			}
		}
		protected virtual void PerformSetParameterValueT<T>(ParameterBinding[] parameters, int p, string name, T value)
		{
			_parameters[p].SpecializedValue = value;
		}	

		public void Initialize(IEnumerable<ParameterBinding> bindings)
		{
			if (bindings != null)
			{
				_parameters = bindings.ToArray();
			}
		}
	}
}
