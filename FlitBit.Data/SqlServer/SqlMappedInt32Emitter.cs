using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedInt32Emitter : SqlDbTypeEmitter<int>
	{
		internal SqlMappedInt32Emitter()
			: base(DbType.Int32, SqlDbType.Int)
		{
		  DbDataReaderGetValueMethodName = "GetInt32";
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
    //  il.NewObj(typeof(SqlInt32).GetConstructor(new [] { typeof(int) }));
    //  il.Box(typeof(SqlInt32));
    //}
	}
}