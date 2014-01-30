using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedByteEmitter : SqlDbTypeEmitter<byte>
	{
		internal SqlMappedByteEmitter()
			: base(DbType.Byte, SqlDbType.TinyInt)
		{
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetByte", typeof(int));
		}

		protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocal(local);
			il.NewObj(typeof (SqlByte).GetConstructor(new[] {typeof (byte)}));
			il.Box(typeof (SqlByte));
		}
	}
}