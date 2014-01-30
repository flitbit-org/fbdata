﻿using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedEmumAsInt32Emitter<TEnum> : SqlDbTypeEmitter<TEnum>
		where TEnum : struct
	{
		internal SqlMappedEmumAsInt32Emitter()
			: base(DbType.Int16, SqlDbType.Int, typeof(int))
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
			il.CallVirtual<DbDataReader>("GetInt32", typeof(int));
		}

		protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocal(local);
			il.NewObj(typeof(SqlInt32).GetConstructor(new[] { typeof(int) }));
			il.Box(typeof(SqlInt32));
		}
	}
}