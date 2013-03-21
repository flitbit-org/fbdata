using System;
using System.Data;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data
{
	/// <summary>
	/// Used by the framework to emit optimized IL for database IO on behalf of data models.
	/// </summary>
	public abstract class MappedDbTypeEmitter
	{						
		protected MappedDbTypeEmitter(DbType dbType, Type type)
		{
			this.DbType = dbType;
			this.RuntimeType = type;
		}

		/// <summary>
		/// Gets the runtime type associated with the mapped DbType.
		/// </summary>
		public Type RuntimeType { get; private set; }

		/// <summary>
		/// Gets the mapping's common DbType.
		/// </summary>
		public DbType DbType { get; private set; }

		/// <summary>
		/// Emits MSIL that loads a value from a DbDataReader, translates it to the RuntimeType, and leaves the value on the stack.
		/// </summary>
		/// <param name="method">the method under construction.</param>
		/// <param name="reader">a reference to the reader.</param>
		/// <param name="columnIndex">a reference to the column's index within the reader.</param>
		/// <param name="details">mapping detail for the column.</param>
		public abstract void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details);
	}

	internal abstract class MappedDbTypeEmitter<T> : MappedDbTypeEmitter
	{
		protected MappedDbTypeEmitter(DbType dbType)
			: base(dbType, typeof(T))
		{}
	}															 

	internal abstract class MappedDbTypeEmitter<T, TDbType> : MappedDbTypeEmitter<T>
		where TDbType: struct
	{
		protected MappedDbTypeEmitter(DbType dbType, TDbType specializedDbType)
			:base(dbType)
		{
			this.SpecializedDbType = specializedDbType;
		} 

		/// <summary>
		/// Gets the mapping's specialized DbType.
		/// </summary>
		public TDbType SpecializedDbType { get; private set; }
	}
}