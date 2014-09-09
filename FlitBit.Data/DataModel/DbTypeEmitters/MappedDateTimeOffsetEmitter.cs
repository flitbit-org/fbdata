#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
    internal class MappedDateTimeOffsetEmitter : MappedDbTypeEmitter<DateTimeOffset, DbType>
    {
        internal MappedDateTimeOffsetEmitter()
            : base(DbType.DateTime, DbType.DateTime) { DbDataReaderGetValueMethodName = "GetDateTimeOffset"; }
    }
}