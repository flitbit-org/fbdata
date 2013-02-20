#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion


using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Core;

namespace FlitBit.Data
{
	
	public partial class BasicDbExecutable : Disposable
	{		
		/// <summary>
		/// Gets the command's parameter definitions.
		/// </summary>
		internal protected IEnumerable<ParameterBinding> Parameters
		{
			get
			{
				if (_parameters == null) Enumerable.Empty<ParameterBinding>();
				return _parameters.ToReadOnly();
			}
		}

		internal void AddParameterBinding(DbParamDefinition def)
		{
			Contract.Requires<InvalidOperationException>(IndexOfParameter(def.Name) < 0, "Parameter already exists; parameter bind names must be unique.");
			ParameterBinding binding = new ParameterBinding();
			binding.Definition = def;
			_parameters = _parameters.Concat(new ParameterBinding[] { binding }).ToArray();
		}

		internal IEnumerable<ParameterBinding> PrepareParametersFromSource(IEnumerable<ParameterBinding> parameters)
		{
			return PerformPrepareParametersFromSource(parameters);
		}
		
		protected virtual IEnumerable<ParameterBinding> PerformPrepareParametersFromSource(IEnumerable<ParameterBinding> parameters)
		{
			return parameters;
		}

		internal protected void PrepareParametersForExecute(IDbCommand command, IEnumerable<ParameterBinding> parameters)
		{
			Contract.Requires<ArgumentNullException>(command != null);
			Contract.Requires<ArgumentNullException>(parameters != null);

			foreach (var binding in parameters)
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
			}
		}
				
		protected virtual string DetermineBindNameForParameter(DbParamDefinition definition)
		{
			return definition.Name;
		}

		protected virtual IDataParameter MakeParameter(IDbCommand command, string name, DbType dbType, int specializedDbType, int length, ParameterDirection direction)
		{
			var result = command.CreateParameter();
			result.ParameterName = name;
			result.DbType = dbType;
			result.Direction = direction;
			return result;
		}
		protected virtual IDataParameter MakeParameter(IDbCommand command, string name, DbType dbType, int specializedDbType, int size, byte scale, ParameterDirection direction)
		{
			var result = command.CreateParameter();
			result.ParameterName = name;
			result.DbType = dbType;
			result.Direction = direction;
			return result;
		}
		protected virtual IDataParameter MakeParameter(IDbCommand command, string name, DbType dbType, int specializedDbType, ParameterDirection direction)
		{
			var result = command.CreateParameter();
			result.ParameterName = name;
			result.DbType = dbType;
			result.Direction = direction;
			return result;
		}
			
		public IDbExecutable DefineParameter(string name, Type runtimeType)
		{
			return DefineParameter(name, runtimeType, ParameterDirection.Input);
		}
		
		public IDbExecutable DefineParameter(string name, Type runtimeType, ParameterDirection direction)
		{
			var bindName = PrepareParameterName(name);
			AddParameterBinding(new DbParamDefinition(name, bindName, runtimeType, direction));
			return this;
		}
		
		public IDbExecutable DefineParameter(string name, Type runtimeType, int length, ParameterDirection direction)
		{
			var bindName = PrepareParameterName(name);
			AddParameterBinding(new DbParamDefinition(name, bindName, runtimeType, length, direction));
			return this;
		}

		public IDbExecutable DefineParameter(string name, DbType dbType)
		{
			return DefineParameter(name, dbType, ParameterDirection.Input);
		}

		public IDbExecutable DefineParameter(string name, DbType dbType, ParameterDirection direction)
		{
			Contract.Assert(name != null);
			Contract.Assert(name.Length > 0);

			var bindName = PrepareParameterName(name);
			
			AddParameterBinding(new DbParamDefinition(name, bindName, dbType, direction));
			return this;
		}

		public IDbExecutable DefineParameter(string name, DbType dbType, int length)
		{
			return DefineParameter(name, dbType, length, ParameterDirection.Input);
		}

		public IDbExecutable DefineParameter(string name, DbType dbType, int length, ParameterDirection direction)
		{
			var bindName = PrepareParameterName(name);
			AddParameterBinding(new DbParamDefinition(name, bindName, dbType, length, direction));
			return this;
		}

		public IDbExecutable DefineParameter(string name, DbType dbType, int size, byte scale)
		{
			return DefineParameter(name, dbType, size, scale, ParameterDirection.Input);
		}

		public IDbExecutable DefineParameter(string name, DbType dbType, int size, byte scale, ParameterDirection direction)
		{
			var bindName = PrepareParameterName(name);
			AddParameterBinding(new DbParamDefinition(name, bindName, dbType, size, scale, direction));
			return this;
		}

		public IDbExecutable DefineParameter(Func<DbParamDefinition> specializeParam)
		{
			var param = specializeParam();
			AddParameterBinding(param);
			return this;
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

		public IDbExecutable SetParameterValue(string name, bool value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		public IDbExecutable SetParameterValue(string name, byte[] value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		public IDbExecutable SetParameterValue(string name, byte value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		public IDbExecutable SetParameterValue(string name, DateTime value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		public IDbExecutable SetParameterValue(string name, decimal value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		public IDbExecutable SetParameterValue(string name, Double value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		public IDbExecutable SetParameterValue(string name, Guid value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		public IDbExecutable SetParameterValue(string name, Single value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		
		public IDbExecutable SetParameterValue(string name, SByte value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		public IDbExecutable SetParameterValue(string name, string value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		public IDbExecutable SetParameterValue(string name, Int16 value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		public IDbExecutable SetParameterValue(string name, Int32 value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		public IDbExecutable SetParameterValue(string name, Int64 value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		
		public IDbExecutable SetParameterValue(string name, UInt16 value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		
		public IDbExecutable SetParameterValue(string name, UInt32 value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		
		public IDbExecutable SetParameterValue(string name, UInt64 value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValue(_parameters, p, name, value);
			return this;
		}
		
		public IDbExecutable SetParameterValue<T>(string name, T value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValueT(_parameters, p, name, value);
			return this;
		}
		public IDbExecutable SetParameterValueAsEnum<E>(string name, E value)
		{
			Contract.Assert(name != null);
			Contract.Assert(typeof(E).IsEnum, "typeof value must be an enum");
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParameterValueAsEnum(_parameters, p, name, value);
			return this;
		}

		public IDbExecutable SetParameterDbNull(string name)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			PerformSetParamterDbNull(_parameters, p);
			return this;
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
		
		protected virtual void PerformSetParameterValueAsEnum<E>(ParameterBinding[] parameters, int p, string name, E value)
		{
			parameters[p].SpecializedValue = Convert.ToInt32(value);			
		}
		protected virtual void PerformSetParameterValueT<T>(ParameterBinding[] parameters, int p, string name, T value)
		{
			parameters[p].SpecializedValue = value;
		}
		protected virtual void PerformSetParamterDbNull(ParameterBinding[] parameters, int p)
		{
			parameters[p].SpecializedValue = DBNull.Value;
		}

		public void GetParameterValueAs<T>(string name, out T value)
		{
			Contract.Assert(name != null);
			int p = IndexOfParameter(name);
			if (p < 0) throw new ArgumentOutOfRangeException(String.Concat("Parameter not defined: ", name));
			var param = (IDataParameter)_command.Parameters[_parameters[p].Definition.BindName];
			value = (T)param.Value;
		}

		protected override bool PerformDispose(bool disposing)
		{
			return disposing;
		}
	}
}
