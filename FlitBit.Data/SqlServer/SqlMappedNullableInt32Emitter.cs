using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedNullableInt32Emitter : SqlDbTypeEmitter<int?>
	{
		internal SqlMappedNullableInt32Emitter()
			: base(DbType.Int32, SqlDbType.Int, typeof(int))
		{
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetInt32", typeof(int));
			EmitTranslateDbType(il);
		}

		protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocalAddress(local);
      il.CallVirtual<int?>("get_Value");
			il.NewObj(typeof(SqlInt32).GetConstructor(new[] { typeof(int) }));
			il.Box(typeof(SqlInt32));
		}


		protected override void EmitTranslateDbType(ILGenerator il)
		{
			il.NewObj(typeof(int?).GetConstructor(new[] { typeof(int) }));
		}
	}
}