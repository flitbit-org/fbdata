using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedEmumAsInt16Emitter<TEnum> : SqlDbTypeEmitter<TEnum>
		where TEnum : struct
	{
		internal SqlMappedEmumAsInt16Emitter()
			: base(DbType.Int16, SqlDbType.SmallInt, typeof(short))
		{
		}

		/// <summary>
		///   Emits MSIL that loads a value from a DbDataReader, translates it to the RuntimeType, and leaves the value on the
		///   stack.
		/// </summary>
		/// <param name="method">the method under construction.</param>
		/// <param name="reader">a reference to the reader.</param>
		/// <param name="columnIndex">a reference to the column's index within the reader.</param>
		/// <param name="details">mapping detail for the column.</param>
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetInt16", typeof(int));
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