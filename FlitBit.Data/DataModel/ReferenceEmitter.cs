using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel
{
	public static class ReferenceEmitter
	{
		public static void BindParameterOnDbCommand<TDbParameter>(MethodBuilder method, MappedDbTypeEmitter emitter,
			ColumnMapping column, string bindingName, Action<ILGenerator> loadCmd, Action<ILGenerator> loadModel,
			LocalBuilder flag)
			where TDbParameter: DbParameter
		{
			ILGenerator il = method.GetILGenerator();
			var details = column.DbTypeDetails;
			var parm = il.DeclareLocal(typeof(TDbParameter));

			il.LoadValue(bindingName);
			il.LoadValue(emitter.SpecializedDbTypeValue);
			il.NewObj(typeof(TDbParameter).GetConstructor(Type.EmptyTypes));
			il.StoreLocal(parm);
			il.LoadLocal(parm);
			il.LoadValue(bindingName);
			il.CallVirtual<TDbParameter>("set_ParameterName");
			emitter.EmitDbParameterSetDbType(il, parm);
			if (emitter.IsLengthRequired)
			{
				il.LoadLocal(parm);
				il.LoadValue(details.Length.Value);
				il.CallVirtual<TDbParameter>("set_Size");
			}
			else if (emitter.IsPrecisionRequired)
			{
				il.LoadLocal(parm);
				il.LoadValue(details.Length.Value);
				il.CallVirtual<TDbParameter>("set_Precision");
				if (emitter.IsScaleRequired || (emitter.IsScaleOptional && details.Scale.HasValue))
				{
					il.LoadLocal(parm);
					il.LoadValue(details.Scale.Value);
					il.CallVirtual<TDbParameter>("set_Scale");
				}
			}

			var local = il.DeclareLocal(column.RuntimeType);

			loadModel(il);
			il.CallVirtual(((PropertyInfo)column.Member).GetGetMethod());
			il.StoreLocal(local);
			emitter.EmitDbParameterSetValue(il, parm, local, flag);

			loadCmd(il);
			il.CallVirtual<DbCommand>("get_Parameters");
			il.LoadLocal(parm);
			il.CallVirtual<DbParameterCollection>("Add", typeof(DbParameter));
			il.Pop();
		}
	}
}
