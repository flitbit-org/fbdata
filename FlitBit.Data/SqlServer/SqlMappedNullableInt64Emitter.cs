using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedNullableInt64Emitter : SqlDbTypeEmitter<long?>
	{
		internal SqlMappedNullableInt64Emitter()
			: base(DbType.Int64, SqlDbType.BigInt, typeof(long))
		{
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetInt64", typeof(int));
			EmitTranslateDbType(il);
		}

		protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocalAddress(local);
      il.CallVirtual<long?>("get_Value");
			il.NewObj(typeof(SqlInt64).GetConstructor(new[] { typeof(long) }));
			il.Box(typeof(SqlInt64));
		}


		protected override void EmitTranslateDbType(ILGenerator il)
		{
			il.NewObj(typeof(long?).GetConstructor(new[] { typeof(long) }));
		}
	}
}