using FlitBit.Emit;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedBoolAsBitEmitter : SqlDbTypeEmitter<bool>
	{
		internal SqlMappedBoolAsBitEmitter()
			: base(DbType.Boolean, SqlDbType.Bit)
		{
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetBoolean", typeof(int));
		}

		protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocal(local);
			il.NewObj(typeof(SqlBoolean).GetConstructor(new[] { typeof(bool) }));
			il.Box(typeof(SqlBoolean));
		}

		protected override string TransformConstantValueToString(object value)
		{
			return (bool) value ? "1" : "0";
		}
	}
}