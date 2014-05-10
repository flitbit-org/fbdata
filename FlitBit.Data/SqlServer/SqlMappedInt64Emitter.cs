#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion
 
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using FlitBit.Data.Meta;
using FlitBit.Emit;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedInt64Emitter : SqlDbTypeEmitter<int>
	{
		internal SqlMappedInt64Emitter()
			: base(DbType.Int64, SqlDbType.BigInt)
		{
		  DbDataReaderGetValueMethodName = "GetInt64";
		}

		public override void EmitColumnInitializationDDL<TModel>(StringBuilder buffer, IMapping<TModel> mapping, ColumnMapping<TModel> col)
		{
			if (col.IsSynthetic)
			{
				buffer.Append(" IDENTITY(1, 1)");
			}
		}

    //protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    //{
    //  il.LoadLocal(local);
    //  il.NewObj(typeof(SqlInt64).GetConstructor(new[] { typeof(long) }));
    //  il.Box(typeof(SqlInt64));
    //}
		
	}
}