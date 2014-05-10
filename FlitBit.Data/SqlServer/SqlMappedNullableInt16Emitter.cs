#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedNullableInt16Emitter : SqlDbTypeNullableEmitter<short>
  {
    internal SqlMappedNullableInt16Emitter()
      : base(DbType.Int16, SqlDbType.SmallInt) { DbDataReaderGetValueMethodName = "GetInt16"; }
  }
}