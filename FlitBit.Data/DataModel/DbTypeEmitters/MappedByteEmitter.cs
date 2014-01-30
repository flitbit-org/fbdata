using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
  internal class MappedByteEmitter : MappedDbTypeEmitter<byte, DbType>
  {
    internal MappedByteEmitter()
      : base(DbType.Byte, DbType.Byte)
    {
      DbDataReaderGetValueMethodName = "GetByte";
    }
  }
}