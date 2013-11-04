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

		/// <summary>
		///   Emits IL to translate the runtime type to the dbtype.
		/// </summary>
		/// <param name="il"></param>
		/// <remarks>
		///   At the time of the call the runtime value is on top of the stack.
		///   When the method returns the translated type must be on the top of the stack.
		/// </remarks>
		protected override void EmitTranslateRuntimeType(ILGenerator il)
		{
			il.NewObj(typeof(SqlBoolean).GetConstructor(new[] { typeof(bool) }));
			il.Box(typeof(SqlBoolean));
		}
	}
}