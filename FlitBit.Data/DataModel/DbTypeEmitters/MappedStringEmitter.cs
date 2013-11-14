using System;
using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedStringEmitter : MappedDbTypeEmitter<string, DbType>
	{
		internal MappedStringEmitter(DbType dbType)
			: base(dbType, dbType)
		{
			switch (dbType)
			{
				case DbType.AnsiString:
					this.SpecializedSqlTypeName = "VARCHAR";
					this.LengthRequirements = DbTypeLengthRequirements.Length;
					break;
				case DbType.String:
					this.SpecializedSqlTypeName = "NVARCHAR";
					this.LengthRequirements = DbTypeLengthRequirements.Length;
					break;
				case DbType.AnsiStringFixedLength:
					this.SpecializedSqlTypeName = "CHAR";
					this.LengthRequirements = DbTypeLengthRequirements.Length;
					break;
				case DbType.StringFixedLength:
					this.SpecializedSqlTypeName = "NCHAR";
					this.LengthRequirements = DbTypeLengthRequirements.Length;
					break;
				default:
					throw new ArgumentOutOfRangeException("dbType");
			}
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
		}
	}
}