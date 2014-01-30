using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedNullableSingleEmitter : SqlDbTypeNullableEmitter<float>
  {
    internal SqlMappedNullableSingleEmitter()
      : base(DbType.Single, SqlDbType.Real)
    {
      DbDataReaderGetValueMethodName = "GetDouble";
    }
  }
}