using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedGuidEmitter : MappedDbTypeEmitter<Guid, SqlDbType>
	{
		internal SqlMappedGuidEmitter()
			: base(default(DbType), SqlDbType.UniqueIdentifier)
		{	
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetGuid", typeof(int));
		}
		public override void EmitColumnConstraintsDDL<TModel>(StringBuilder buffer, Mapping<TModel> mapping, ColumnMapping<TModel> col, System.Collections.Generic.List<string> tableConstraints)
		{
			if (col.IsIdentity && mapping.Identity.Columns.Count() == 1)
			{
				buffer.Append(Environment.NewLine)
							.Append("\t\tCONSTRAINT DF_")
							.Append(mapping.TargetSchema)
							.Append(mapping.TargetObject)
							.Append('_')
							.Append(col.TargetName)
							.Append(" DEFAULT (NEWID())");
			}
			base.EmitColumnConstraintsDDL(buffer, mapping, col, tableConstraints);
		}
	}
	internal class SqlMappedInt32Emitter : MappedDbTypeEmitter<int, SqlDbType>
	{
		internal SqlMappedInt32Emitter()
			: base(default(DbType), SqlDbType.Int)
		{
		}
		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetInt32", typeof(int));
		}
		public override void EmitColumnInitializationDDL<TModel>(StringBuilder buffer, Mapping<TModel> mapping, ColumnMapping<TModel> col)
		{
			if (col.IsSynthetic)
			{
				buffer.Append(" IDENTITY(1, 1)");
			}
		}
	}
	internal class SqlMappedInt64Emitter : MappedDbTypeEmitter<int, SqlDbType>
	{
		internal SqlMappedInt64Emitter()
			: base(default(DbType), SqlDbType.BigInt)
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
