using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedEmumAsInt16Emitter<TEnum> : MappedDbTypeEmitter<TEnum, DbType>
		where TEnum: struct
	{
		internal MappedEmumAsInt16Emitter()
			: base(DbType.Int16, DbType.Int16)
		{
			this.SpecializedSqlTypeName = "SMALLINT";
		  DbDataReaderGetValueMethodName = "GetInt16";
		}

		protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocal(local);
			il.Call(typeof(Convert).GetMethod("ToInt16", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object) }, null));
		}
	}
}