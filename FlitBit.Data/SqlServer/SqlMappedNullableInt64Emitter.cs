using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedNullableInt64Emitter : SqlDbTypeNullableEmitter<long>
	{
		internal SqlMappedNullableInt64Emitter()
			: base(DbType.Int64, SqlDbType.BigInt)
		{
		  DbDataReaderGetValueMethodName = "GetInt64";
		}

	}
}