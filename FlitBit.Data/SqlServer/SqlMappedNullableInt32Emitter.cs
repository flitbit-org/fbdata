#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;

namespace FlitBit.Data.SqlServer
{
    internal class SqlMappedNullableInt32Emitter : SqlDbTypeNullableEmitter<int>
    {
        internal SqlMappedNullableInt32Emitter()
            : base(DbType.Int32, SqlDbType.Int) { DbDataReaderGetValueMethodName = "GetInt32"; }
    }
}