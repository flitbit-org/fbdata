using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{

  internal class SqlMappedNullableDoubleEmitter : SqlDbTypeNullableEmitter<decimal>
  {
    internal SqlMappedNullableDoubleEmitter()
      : base(DbType.Double, SqlDbType.Float)
    {
      DbDataReaderGetValueMethodName = "GetDouble";
    }
  }
}