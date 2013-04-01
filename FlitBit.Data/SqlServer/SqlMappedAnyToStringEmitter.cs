﻿using System;
using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedAnyToStringEmitter<T> : MappedDbTypeEmitter<T, SqlDbType>
	{
		internal SqlMappedAnyToStringEmitter(SqlDbType dbType)
			: base(default(DbType), dbType)
		{
			switch (dbType)
			{
				case SqlDbType.VarChar:
					this.DbType = DbType.AnsiString;
					this.LengthRequirements = DbTypeLengthRequirements.Length;
					break;
				case SqlDbType.NVarChar:
					this.DbType = DbType.String;
					this.LengthRequirements = DbTypeLengthRequirements.Length;
					break;
				case SqlDbType.Char:
					this.DbType = DbType.AnsiStringFixedLength;
					this.LengthRequirements = DbTypeLengthRequirements.Length;
					break;
				case SqlDbType.NChar:
					this.DbType = DbType.StringFixedLength;
					this.LengthRequirements = DbTypeLengthRequirements.Length;
					break;
				default:
					throw new ArgumentOutOfRangeException("dbType");
			}
			this.TreatMissingLengthAsMaximum = true;
			this.LengthMaximum = "MAX";
		}

		/// <summary>
		/// Emits MSIL that loads a value from a DbDataReader, translates it to the RuntimeType, and leaves the value on the stack.
		/// </summary>
		/// <param name="method">the method under construction.</param>
		/// <param name="reader">a reference to the reader.</param>
		/// <param name="columnIndex">a reference to the column's index within the reader.</param>
		/// <param name="details"></param>
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetString", typeof(int));
			EmitTranslateType(method);
		}

		protected virtual void EmitTranslateType(MethodBuilder method)
		{
			// default to nothing, assuming the string type is ok.
		}
	}
}