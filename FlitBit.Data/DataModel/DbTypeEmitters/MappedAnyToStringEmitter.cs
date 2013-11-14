using System.Data;
using System.Reflection.Emit;
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

		protected virtual void EmitTranslateType(MethodBuilder method)
		{
			// default to nothing, assuming the string type is ok.
		}
	}
}