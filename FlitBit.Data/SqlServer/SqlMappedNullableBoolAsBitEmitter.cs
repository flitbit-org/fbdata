using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedNullableBoolAsBitEmitter : SqlDbTypeEmitter<bool?>
	{
		internal SqlMappedNullableBoolAsBitEmitter()
			: base(DbType.Boolean, SqlDbType.Bit)
		{
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetBoolean", typeof(int));
			EmitTranslateDbType(il);
		}

    protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocalAddress(local);
      il.CallVirtual<bool?>("get_Value");
			il.NewObj(typeof(SqlBoolean).GetConstructor(new[] { typeof(bool) }));
			il.Box(typeof(SqlBoolean));
		}

		protected override void EmitTranslateDbType(ILGenerator il)
		{
			il.NewObj(typeof(bool?).GetConstructor(new[] { typeof(bool) }));
		}

		protected override string TransformConstantValueToString(object value)
		{
			return (bool)value ? "1" : "0";
		}
	}
}