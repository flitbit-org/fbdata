using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedNullableByteEmitter : SqlDbTypeEmitter<byte?>
	{
		internal SqlMappedNullableByteEmitter()
			: base(DbType.Byte, SqlDbType.TinyInt, typeof(byte))
		{
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetByte", typeof(int));
			EmitTranslateDbType(il);
		}

    protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocalAddress(local);
      il.CallVirtual<byte?>("get_Value");
			il.NewObj(typeof(SqlByte).GetConstructor(new[] { typeof(byte) }));
			il.Box(typeof(SqlByte));
		}


		protected override void EmitTranslateDbType(ILGenerator il)
		{
			il.NewObj(typeof(byte?).GetConstructor(new[] { typeof(byte) }));
		}
	}
}