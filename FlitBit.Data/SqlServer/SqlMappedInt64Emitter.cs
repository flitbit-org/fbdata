using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedInt64Emitter : SqlDbTypeEmitter<int>
	{
		internal SqlMappedInt64Emitter()
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
		public override void EmitColumnInitializationDDL<TModel>(StringBuilder buffer, Mapping<TModel> mapping, ColumnMapping<TModel> col)
		{
			if (col.IsSynthetic)
			{
				buffer.Append(" IDENTITY(1, 1)");
			}
		}
		
	}
}