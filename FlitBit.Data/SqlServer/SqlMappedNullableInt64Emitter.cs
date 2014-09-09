#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;

namespace FlitBit.Data.SqlServer
{
    internal class SqlMappedNullableInt64Emitter : SqlDbTypeNullableEmitter<long>
    {
        internal SqlMappedNullableInt64Emitter()
            : base(DbType.Int64, SqlDbType.BigInt) { DbDataReaderGetValueMethodName = "GetInt64"; }
    }
}