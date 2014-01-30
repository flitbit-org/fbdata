using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedNullableInt32Emitter : SqlDbTypeNullableEmitter<int>
	{
		internal SqlMappedNullableInt32Emitter()
			: base(DbType.Int32, SqlDbType.Int)
		{
		  DbDataReaderGetValueMethodName = "GetInt32";
		}
	}
}