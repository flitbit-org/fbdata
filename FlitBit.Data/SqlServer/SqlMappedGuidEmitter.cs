﻿using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedGuidEmitter : SqlDbTypeEmitter<Guid>
	{
		internal SqlMappedGuidEmitter()
			: base(DbType.Guid, SqlDbType.UniqueIdentifier)
		{
			this.IsQuoteRequired = true;
			this.QuoteChars = "'";
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

		/// <summary>
		///   Emits IL to translate the runtime type to the dbtype.
		/// </summary>
		/// <param name="il"></param>
		/// <remarks>
		///   At the time of the call the runtime value is on top of the stack.
		///   When the method returns the translated type must be on the top of the stack.
		/// </remarks>
		protected override void EmitTranslateRuntimeType(ILGenerator il)
		{
			il.NewObj(typeof(SqlGuid).GetConstructor(new[] { typeof(Guid) }));
			il.Box(typeof(SqlGuid));
		}
	}
}
