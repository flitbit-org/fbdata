#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;

namespace FlitBit.Data.SqlServer
{
    internal class SqlMappedDateTimeEmitter : SqlDbTypeEmitter<DateTime>
    {
        internal SqlMappedDateTimeEmitter(SqlDbType dbType)
            : base(default(DbType), dbType)
        {
            switch (dbType)
            {
                case SqlDbType.Date:
                    this.DbType = DbType.Date;
                    this.SpecializedSqlTypeName = "DATE";
                    this.LengthRequirements = DbTypeLengthRequirements.None;
                    break;
                case SqlDbType.DateTime:
                    this.DbType = DbType.DateTime;
                    this.LengthRequirements = DbTypeLengthRequirements.None;
                    break;
                case SqlDbType.DateTime2:
                    this.DbType = DbType.DateTime2;
                    this.LengthRequirements = DbTypeLengthRequirements.Length;
                    this.TreatMissingLengthAsMaximum = true;
                    this.LengthMaximum = "7";
                    break;
                case SqlDbType.Time:
                    this.DbType = DbType.Time;
                    this.LengthRequirements = DbTypeLengthRequirements.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("dbType");
            }

            DbDataReaderGetValueMethodName = "GetDateTime";
        }
    }
}