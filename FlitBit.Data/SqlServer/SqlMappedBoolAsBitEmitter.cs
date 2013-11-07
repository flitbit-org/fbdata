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

		protected override string TransformConstantValueToString(object value)
		{
			return (bool) value ? "1" : "0";
		}
	}


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
			il.NewObj(typeof (SqlByte).GetConstructor(new[] {typeof (byte)}));
			il.Box(typeof (SqlByte));
		}
	}


	internal class SqlMappedNullableByteEmitter : SqlDbTypeEmitter<byte?>
	{
		internal SqlMappedNullableByteEmitter()
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
			il.NewObj(typeof(SqlByte).GetConstructor(new[] { typeof(byte) }));
			il.Box(typeof(SqlByte));
		}


		protected override void EmitTranslateDbType(ILGenerator il)
		{
			il.NewObj(typeof(byte?).GetConstructor(new[] { typeof(byte) }));
		}
	}


	internal class SqlMappedNullableInt16Emitter : SqlDbTypeEmitter<short?>
	{
		internal SqlMappedNullableInt16Emitter()
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


		protected override void EmitTranslateDbType(ILGenerator il)
		{
			il.NewObj(typeof(short?).GetConstructor(new[] { typeof(short) }));
		}
	}

	internal class SqlMappedNullableInt32Emitter : SqlDbTypeEmitter<int?>
	{
		internal SqlMappedNullableInt32Emitter()
			: base(DbType.Int32, SqlDbType.Int)
		{
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetInt32", typeof(int));
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
			il.NewObj(typeof(SqlInt32).GetConstructor(new[] { typeof(int) }));
			il.Box(typeof(SqlInt32));
		}


		protected override void EmitTranslateDbType(ILGenerator il)
		{
			il.NewObj(typeof(int?).GetConstructor(new[] { typeof(int) }));
		}
	}

	
	internal class SqlMappedNullableInt64Emitter : SqlDbTypeEmitter<long?>
	{
		internal SqlMappedNullableInt64Emitter()
			: base(DbType.Int64, SqlDbType.BigInt)
		{
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetInt64", typeof(int));
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
			il.NewObj(typeof(SqlInt64).GetConstructor(new[] { typeof(long) }));
			il.Box(typeof(SqlInt64));
		}


		protected override void EmitTranslateDbType(ILGenerator il)
		{
			il.NewObj(typeof(long?).GetConstructor(new[] { typeof(long) }));
		}
	}
}