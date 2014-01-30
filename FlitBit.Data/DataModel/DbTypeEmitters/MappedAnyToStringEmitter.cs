using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Reflection.Emit;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal abstract class MappedAnyToStringEmitter<T> : MappedDbTypeEmitter<T, DbType>
	{
		readonly MappedStringEmitter _stringEmitter;

		internal MappedAnyToStringEmitter(DbType dbType)
			: base(dbType, dbType)
		{
			this._stringEmitter = new MappedStringEmitter(dbType);
			this.SpecializedSqlTypeName = _stringEmitter.SpecializedSqlTypeName;
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
			this._stringEmitter.LoadValueFromDbReader(method, reader, columnIndex, details);
			EmitTranslateType(method);
		}

    protected internal override void EmitDbParameterSetValue(ILGenerator il, ColumnMapping column, LocalBuilder parm,
      LocalBuilder local, LocalBuilder flag)
    {
      Label fin = il.DefineLabel();
      Label gotoSetNonNullValue = il.DefineLabel();

      // if (field != null) {

      il.LoadLocal(local);
      il.LoadNull();
      il.CompareEqual();
      il.LoadValue(0);
      il.CompareEqual();
      il.StoreLocal(flag);
      il.LoadLocal(flag);
      il.BranchIfTrue(gotoSetNonNullValue);

      //   field.Value = DBNull.Value;

      il.LoadLocal(parm);
      il.LoadField(typeof(DBNull).GetField("Value", BindingFlags.Static | BindingFlags.Public));
      EmitInvokeDbParameterSetValue(il);
      il.Branch(fin);
      
      // } else {
      //   field.Value = field;

      il.MarkLabel(gotoSetNonNullValue);
      il.LoadLocal(parm);
      EmitTranslateRuntimeType(il, local);
      EmitInvokeDbParameterSetValue(il);

      // }
      il.MarkLabel(fin);
    }

		protected virtual void EmitTranslateType(MethodBuilder method)
		{
			// default to nothing, assuming the string type is ok.
		}
	}
}