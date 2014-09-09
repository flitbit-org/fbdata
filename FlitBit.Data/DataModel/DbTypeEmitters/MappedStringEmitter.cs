#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
    internal class MappedStringEmitter : MappedDbTypeEmitter<string, DbType>
    {
        internal MappedStringEmitter(DbType dbType)
            : base(dbType, dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    this.SpecializedSqlTypeName = "VARCHAR";
                    this.LengthRequirements = DbTypeLengthRequirements.Length;
                    break;
                case DbType.String:
                    this.SpecializedSqlTypeName = "NVARCHAR";
                    this.LengthRequirements = DbTypeLengthRequirements.Length;
                    break;
                case DbType.AnsiStringFixedLength:
                    this.SpecializedSqlTypeName = "CHAR";
                    this.LengthRequirements = DbTypeLengthRequirements.Length;
                    break;
                case DbType.StringFixedLength:
                    this.SpecializedSqlTypeName = "NCHAR";
                    this.LengthRequirements = DbTypeLengthRequirements.Length;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("dbType");
            }
            DbDataReaderGetValueMethodName = "GetString";
        }
    }
}