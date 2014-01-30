using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
  internal class MappedUInt64Emitter : MappedDbTypeEmitter<uint, DbType>
  {
    internal MappedUInt64Emitter()
      : base(DbType.UInt64, DbType.UInt64)
    {
      DbDataReaderGetValueMethodName = "GetUInt64";
    }
  }
}