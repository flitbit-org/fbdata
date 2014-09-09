#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;
using System.Text;

namespace FlitBit.Data.SqlServer
{
    internal class SqlDbTypeTranslation : DbTypeTranslation
    {
        public SqlDbTypeTranslation(DbType dbType, SqlDbType specializedDbType, Type runtimeType,
            bool isDefaultForRuntimeType)
            : base(dbType, (int)specializedDbType, runtimeType, isDefaultForRuntimeType,
                specializedDbType.ToString()
                                 .ToUpper(), DbTypeLengthRequirements.None)
        {}

        public SqlDbTypeTranslation(DbType dbType, SqlDbType specializedDbType, Type runtimeType,
            bool isDefaultForRuntimeType, DbTypeLengthRequirements lengthRequirements)
            : base(dbType, (int)specializedDbType, runtimeType, isDefaultForRuntimeType,
                specializedDbType.ToString()
                                 .ToUpper(), lengthRequirements)
        {}

        public SqlDbTypeTranslation(DbType dbType, SqlDbType specializedDbType, Type runtimeType,
            bool isDefaultForRuntimeType, string providerSqlTypeName, DbTypeLengthRequirements lengthRequirements)
            : base(dbType, (int)specializedDbType, runtimeType, isDefaultForRuntimeType,
                providerSqlTypeName, lengthRequirements)
        {}

        public SqlDbTypeTranslation(DbType dbType, SqlDbType specializedDbType, Type runtimeType,
            bool isDefaultForRuntimeType, DbTypeLengthRequirements lengthRequirements,
            string defaultLength)
            : base(dbType, (int)specializedDbType, runtimeType, isDefaultForRuntimeType,
                specializedDbType.ToString()
                                 .ToUpper(), lengthRequirements, defaultLength)
        {}

        public SqlDbTypeTranslation(DbType dbType, SqlDbType specializedDbType, Type runtimeType,
            bool isDefaultForRuntimeType, string providerSqlTypeName, DbTypeLengthRequirements lengthRequirements,
            string defaultLength)
            : base(dbType, (int)specializedDbType, runtimeType, isDefaultForRuntimeType,
                providerSqlTypeName, lengthRequirements, defaultLength)
        {}

        public override bool MustWriteLength(int declaredLength, int declaredScale)
        {
            return (this.LengthRequirements & (DbTypeLengthRequirements.LengthSpecifierMask))
                   != DbTypeLengthRequirements.None;
        }

        public override void WriteLength(int declaredLength, int declaredScale, StringBuilder sql)
        {
            var len = declaredLength;
            if (RuntimeType == typeof(decimal)
                && len == 0)
            {
                return;
            }
            sql.Append('(');
            if ((this.LengthRequirements & DbTypeLengthRequirements.Length) == DbTypeLengthRequirements.Length
                || (this.LengthRequirements & DbTypeLengthRequirements.Precision) == DbTypeLengthRequirements.Precision)
            {
                if (len == 0)
                {
                    sql.Append(this.DefaultLength);
                }
                else
                {
                    sql.Append(len);
                }
            }
            if ((this.LengthRequirements & DbTypeLengthRequirements.Scale) == DbTypeLengthRequirements.Scale)
            {
#warning TODO: implement scale on col and type translations.
            }
            sql.Append(')');
        }
    }
}