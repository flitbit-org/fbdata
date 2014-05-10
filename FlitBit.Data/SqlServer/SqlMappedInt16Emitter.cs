#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using System.Text;
using FlitBit.Data.Meta;
using FlitBit.Emit;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedInt16Emitter : SqlDbTypeEmitter<short>
	{
		internal SqlMappedInt16Emitter()
			: base(DbType.Int16, SqlDbType.SmallInt)
		{
		  DbDataReaderGetValueMethodName = "GetInt16";
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
    //  il.NewObj(typeof(SqlInt16).GetConstructor(new[] { typeof(short) }));
    //  il.Box(typeof(SqlInt16));
    //}
	}
}