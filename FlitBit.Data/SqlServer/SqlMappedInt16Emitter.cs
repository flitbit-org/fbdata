using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using System.Text;
using FlitBit.Data.Meta;
using FlitBit.Emit;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedInt16Emitter : SqlDbTypeEmitter<short>
	{
		internal SqlMappedInt16Emitter()
			: base(DbType.Int16, SqlDbType.SmallInt)
		{
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetInt16", typeof(int));
		}
		public override void EmitColumnInitializationDDL<TModel>(StringBuilder buffer, IMapping<TModel> mapping, ColumnMapping<TModel> col)
		{
			if (col.IsSynthetic)
			{
				buffer.Append(" IDENTITY(1, 1)");
			}
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
			il.NewObj(typeof(SqlInt16).GetConstructor(new[] { typeof(short) }));
			il.Box(typeof(SqlInt16));
		}
	}
}