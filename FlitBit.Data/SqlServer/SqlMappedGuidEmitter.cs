using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Emit;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedGuidEmitter : SqlDbTypeEmitter<Guid>
	{
		internal SqlMappedGuidEmitter()
			: base(DbType.Guid, SqlDbType.UniqueIdentifier)
		{
			this.IsQuoteRequired = true;
			this.QuoteChars = "'";
		  DbDataReaderGetValueMethodName = "GetGuid";
		}

		public override void EmitColumnConstraintsDDL<TModel>(StringBuilder buffer, IMapping<TModel> mapping, ColumnMapping<TModel> col, System.Collections.Generic.List<string> tableConstraints)
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

    //protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    //{
    //  il.LoadLocal(local);
    //  il.NewObj(typeof(SqlGuid).GetConstructor(new[] { typeof(Guid) }));
    //  il.Box(typeof(SqlGuid));
    //}
	}

	internal class SqlMappedNullableGuidEmitter : SqlDbTypeEmitter<Guid>
	{
		internal SqlMappedNullableGuidEmitter()
			: base(DbType.Guid, SqlDbType.UniqueIdentifier)
		{
			this.IsQuoteRequired = true;
			this.QuoteChars = "'";
		}

		public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex,
			DbTypeDetails details)
		{
			var il = method.GetILGenerator();
			reader.LoadValue(il);
			columnIndex.LoadValue(il);
			il.CallVirtual<DbDataReader>("GetGuid", typeof (int));
		}

		public override void EmitColumnConstraintsDDL<TModel>(StringBuilder buffer, IMapping<TModel> mapping,
			ColumnMapping<TModel> col, System.Collections.Generic.List<string> tableConstraints)
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

    protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocal(local);
      il.NewObj(typeof(SqlGuid).GetConstructor(new[] { typeof(Guid) }));
			il.Box(typeof (SqlGuid));
		}

		protected override void EmitTranslateDbType(ILGenerator il)
		{
			il.NewObj(typeof (Guid?).GetConstructor(new[] {typeof (Guid)}));
		}
	}
}
