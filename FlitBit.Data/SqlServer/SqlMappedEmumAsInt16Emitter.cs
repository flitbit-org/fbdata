using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedEmumAsInt16Emitter<TEnum> : SqlDbTypeEmitter<TEnum>
		where TEnum : struct
	{
		internal SqlMappedEmumAsInt16Emitter()
			: base(DbType.Int16, SqlDbType.SmallInt, typeof(short))
		{
		  DbDataReaderGetValueMethodName = "GetInt16";
		}

    //protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    //{
    //  il.LoadLocal(local);
    //  il.NewObj(typeof(SqlInt16).GetConstructor(new[] { typeof(short) }));
    //  il.Box(typeof(SqlInt16));
    //}
	}
}