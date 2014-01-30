using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedNullableInt16Emitter : SqlDbTypeEmitter<short?>
	{
		internal SqlMappedNullableInt16Emitter()
			: base(DbType.Int16, SqlDbType.SmallInt, typeof(short))
		{
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetInt16", typeof(int));
			EmitTranslateDbType(il);
		}

		protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocalAddress(local);
      il.CallVirtual<short?>("get_Value");
			il.NewObj(typeof(SqlInt16).GetConstructor(new[] { typeof(short) }));
			il.Box(typeof(SqlInt16));
		}

		protected override void EmitTranslateDbType(ILGenerator il)
		{
			il.NewObj(typeof(short?).GetConstructor(new[] { typeof(short) }));
		}
	}
}