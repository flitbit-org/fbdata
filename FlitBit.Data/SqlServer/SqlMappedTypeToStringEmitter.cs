using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedTypeToStringEmitter : SqlMappedAnyToStringEmitter<Type>
	{
		public SqlMappedTypeToStringEmitter()
			: base(SqlDbType.NVarChar)
		{ }

		protected override void EmitTranslateDbType(ILGenerator il)
		{
			il.Call<Type>("GetType", BindingFlags.Static | BindingFlags.Public, typeof(string));
		}

		protected internal override void EmitTranslateRuntimeType(ILGenerator il)
		{
			il.CallVirtual<Type>("get_FullName");
			base.EmitTranslateRuntimeType(il);
		}

		public override DbTypeDetails GetDbTypeDetails(ColumnMapping column)
		{
			Debug.Assert(column.Member.DeclaringType != null, "column.Member.DeclaringType != null");
			var bindingName = String.Concat(column.Member.DeclaringType.Name, "_", column.Member.Name);
			return new DbTypeDetails(column.Member.Name, bindingName, 400, null);
		}

	}
}
