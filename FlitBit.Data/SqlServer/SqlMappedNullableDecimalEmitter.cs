using System.Data;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedNullableDecimalEmitter : SqlDbTypeNullableEmitter<decimal>
  {
    internal SqlMappedNullableDecimalEmitter()
      : base(DbType.Decimal, SqlDbType.Decimal)
    {
      this.LengthRequirements = DbTypeLengthRequirements.Precision | DbTypeLengthRequirements.Scale | DbTypeLengthRequirements.OptionalScale;
      this.DbDataReaderGetValueMethodName = "GetDecimal";
    }
  }
}