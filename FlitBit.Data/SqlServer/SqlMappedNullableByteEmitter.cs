#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

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