#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedNullableBoolAsBitEmitter : SqlDbTypeNullableEmitter<bool>
  {
    internal SqlMappedNullableBoolAsBitEmitter()
      : base(DbType.Boolean, SqlDbType.Bit) { DbDataReaderGetValueMethodName = "GetBoolean"; }

    protected override string TransformConstantValueToString(object value) { return ((bool?)value).GetValueOrDefault() ? "1" : "0"; }
  }
}