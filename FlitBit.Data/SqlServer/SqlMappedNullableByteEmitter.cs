using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedNullableByteEmitter : SqlDbTypeNullableEmitter<byte>
	{
		internal SqlMappedNullableByteEmitter()
			: base(DbType.Byte, SqlDbType.TinyInt)
		{
		  DbDataReaderGetValueMethodName = "GetByte";
		}

	}
}