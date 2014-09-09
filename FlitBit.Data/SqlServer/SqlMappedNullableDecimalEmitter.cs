#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;

namespace FlitBit.Data.SqlServer
{
    internal class SqlMappedNullableDecimalEmitter : SqlDbTypeNullableEmitter<decimal>
    {
        internal SqlMappedNullableDecimalEmitter()
            : base(DbType.Decimal, SqlDbType.Decimal)
        {
            this.LengthRequirements = DbTypeLengthRequirements.Precision | DbTypeLengthRequirements.Scale
                                      | DbTypeLengthRequirements.OptionalScale;
            this.DbDataReaderGetValueMethodName = "GetDecimal";
        }
    }
}