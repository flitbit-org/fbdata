#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedNullableDoubleEmitter : SqlDbTypeNullableEmitter<decimal>
  {
    internal SqlMappedNullableDoubleEmitter()
      : base(DbType.Double, SqlDbType.Float) { DbDataReaderGetValueMethodName = "GetDouble"; }
  }
}