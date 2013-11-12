using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection;
using System.Reflection.Emit;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedAnyToStringEmitter<T> : SqlDbTypeEmitter<T>
	{
		internal SqlMappedAnyToStringEmitter(SqlDbType dbType)
			: this(dbType, typeof(T))
		{
		}
		internal SqlMappedAnyToStringEmitter(SqlDbType dbType, Type underlying)
			: base(default(DbType), dbType, underlying)
		{
			switch (dbType)
			{
				case SqlDbType.VarChar:
					DbType = DbType.AnsiString;
					LengthRequirements = DbTypeLengthRequirements.Length;
					break;
				case SqlDbType.NVarChar:
					DbType = DbType.String;
					LengthRequirements = DbTypeLengthRequirements.Length;
					break;
				case SqlDbType.Char:
					DbType = DbType.AnsiStringFixedLength;
					LengthRequirements = DbTypeLengthRequirements.Length;
					break;
				case SqlDbType.NChar:
					DbType = DbType.StringFixedLength;
					LengthRequirements = DbTypeLengthRequirements.Length;
					break;
				default:
					throw new ArgumentOutOfRangeException("dbType");
			}
			TreatMissingLengthAsMaximum = true;
			LengthMaximum = "MAX";
		}

		/// <summary>
		///   Emits MSIL that loads a value from a DbDataReader, translates it to the RuntimeType, and leaves the value on the
		///   stack.
		/// </summary>
		/// <param name="method">the method under construction.</param>
		/// <param name="reader">a reference to the reader.</param>
		/// <param name="columnIndex">a reference to the column's index within the reader.</param>
		/// <param name="details"></param>
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex,
			DbTypeDetails details)
		{
			ILGenerator il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetString", typeof (int));
			EmitTranslateDbType(il);
		}

		protected internal virtual void EmitTranslateRuntimeType(ILGenerator il)
		{
			il.NewObj(typeof (SqlString).GetConstructor(new[] {typeof (string)}));
			il.Box(typeof (SqlString));
		}

		protected internal override void EmitDbParameterSetValue(ILGenerator il, ColumnMapping column, LocalBuilder parm,
			LocalBuilder local, LocalBuilder flag)
		{
			Label fin = il.DefineLabel();
			Label ifelse = il.DefineLabel();
			il.LoadLocal(local);
			il.LoadNull();
			il.CompareEqual();
			il.LoadValue(0);
			il.CompareEqual();
			il.StoreLocal(flag);
			il.LoadLocal(flag);
			il.BranchIfTrue(ifelse);
			il.LoadLocal(parm);
			il.LoadField(typeof (DBNull).GetField("Value", BindingFlags.Static | BindingFlags.Public));
			il.CallVirtual<SqlParameter>("set_SqlValue");
			il.Branch(fin);
			il.MarkLabel(ifelse);
			il.LoadLocal(parm);
			il.LoadLocal(local);
			EmitTranslateRuntimeType(il);
			il.CallVirtual<SqlParameter>("set_SqlValue", typeof(object));
			il.MarkLabel(fin);
		}
	}
}