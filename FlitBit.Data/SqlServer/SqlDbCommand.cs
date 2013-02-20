#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion


using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics.Contracts;
using FlitBit.Core.Properties;

namespace FlitBit.Data.SqlServer																				 
{	
	public class SqlDbCommand : BasicDbExecutable
	{
		public SqlDbCommand() : base() { }
		public SqlDbCommand(string connection) : base(connection) { }
		public SqlDbCommand(string commandText, CommandType commandType) 
			: base(commandText, commandType)
		{
		}
		public SqlDbCommand(string connection, string commandText, CommandType commandType)
			: base(connection, commandText, commandType)
		{
		}		
		public SqlDbCommand(string connection, BasicDbExecutable definition) 
			: base(connection, definition)
		{
		}
		public SqlDbCommand(IDbConnection connection, BasicDbExecutable definition)
			: base(connection, definition)
		{
		}

		public override string PrepareParameterName(string name)
		{
			Contract.Assert(name != null);
			Contract.Assert(name.Length > 0);
			return (name[0] != '@') ? String.Concat('@', name) : name;
		}

		protected override IDataParameter MakeParameter(IDbCommand command, string name, DbType dbType, int specializedDbType, int length, ParameterDirection direction)
		{
			SqlCommand scmd = (SqlCommand)command;
			SqlParameter parm = scmd.Parameters.Add(name, (SqlDbType)specializedDbType, length);
			parm.Direction = direction;
			return parm;
		}
		protected override IDataParameter MakeParameter(IDbCommand command, string name, DbType dbType, int specializedDbType, int size, byte scale, ParameterDirection direction)
		{
			SqlCommand scmd = (SqlCommand)command;
			SqlParameter parm = scmd.Parameters.Add(name, (SqlDbType)specializedDbType, size);
			parm.Scale = scale;
			parm.Direction = direction;
			return parm;
		}
		protected override IDataParameter MakeParameter(IDbCommand command, string name, DbType dbType, int specializedDbType, ParameterDirection direction)
		{
			SqlCommand scmd = (SqlCommand)command;
			SqlParameter parm = scmd.Parameters.Add(name, (SqlDbType)specializedDbType);
			parm.Direction = direction;
			return parm;
		}

		protected override IEnumerable<ParameterBinding> PerformPrepareParametersFromSource(IEnumerable<ParameterBinding> parameters)
		{
			List<ParameterBinding> bindings = new List<ParameterBinding>(parameters.Count());
			foreach (var binding in parameters)
			{
				var translation = default(DbTypeTranslation);
				var dbt = binding.Definition.DbType;
				if (dbt == DbParamDefinition.CInvalidDbType)
				{
					if (binding.Definition.RuntimeType != null)
					{
						translation = SqlDbTypeTranslations.TranslateRuntimeType(binding.Definition.RuntimeType);
					}
				}
				else
				{
					translation = SqlDbTypeTranslations.TranslateDbType(dbt);
				}				
				if (translation == null)
				{
					throw new InvalidOperationException(String.Concat(this.GetType().FullName,
						" could not translate parameter definition ", binding.Definition.Name));
				}
				var bindingName = binding.Definition.BindName;
				if (String.IsNullOrEmpty(bindingName))
				{
					bindingName = DetermineBindNameForParameter(binding.Definition);
				}
				var specialized = new DbParamDefinition(binding.Definition.Name,
					PrepareParameterName(bindingName), 
					translation.DbType,
					translation.SpecializedDbType, 
					binding.Definition.Size, 
					binding.Definition.Scale, 
					binding.Definition.Direction);
				bindings.Add(new ParameterBinding 
					{ 
						Definition = specialized,
						SpecializedValue = binding.SpecializedValue
					});
			}
			return bindings.ToArray();
		}

		protected override void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, bool value)
		{
			parameters[p].SpecializedValue = new SqlBoolean(value);
		}
		protected override void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, byte value)
		{
			parameters[p].SpecializedValue = new SqlByte(value);
		}
		protected override void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, byte[] value)
		{
			parameters[p].SpecializedValue = new SqlBytes(value);
		}
		protected override void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, decimal value)
		{
			parameters[p].SpecializedValue = new SqlDecimal(value);
		}
		protected override void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, double value)
		{
			parameters[p].SpecializedValue = new SqlDouble(value);
		}
		protected override void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, DateTime value)
		{
			parameters[p].SpecializedValue = new SqlDateTime(value);
		}
		protected override void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, float value)
		{
			parameters[p].SpecializedValue = new SqlSingle(value);
		}
		protected override void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, Guid value)
		{
			parameters[p].SpecializedValue = new SqlGuid(value);
		}
		protected override void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, int value)
		{
			parameters[p].SpecializedValue = new SqlInt32(value);
		}
		protected override void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, long value)
		{
			parameters[p].SpecializedValue = new SqlInt64(value);
		}
		protected override void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, short value)
		{
			parameters[p].SpecializedValue = new SqlInt16(value);
		}
		protected override void PerformSetParameterValue(ParameterBinding[] parameters, int p, string name, string value)
		{
			parameters[p].SpecializedValue = new SqlString(value);
		}
	}
}
