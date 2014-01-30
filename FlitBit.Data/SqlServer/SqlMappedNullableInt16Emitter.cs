using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedNullableInt16Emitter : SqlDbTypeNullableEmitter<short>
	{
		internal SqlMappedNullableInt16Emitter()
			: base(DbType.Int16, SqlDbType.SmallInt)
		{
		  DbDataReaderGetValueMethodName = "GetInt16";
		}
	}
}