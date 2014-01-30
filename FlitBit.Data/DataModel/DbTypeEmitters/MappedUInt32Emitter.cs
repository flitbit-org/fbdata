using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
  internal class MappedUInt32Emitter : MappedDbTypeEmitter<uint, DbType>
  {
    internal MappedUInt32Emitter()
      : base(DbType.UInt32, DbType.UInt32)
    {
      this.SpecializedSqlTypeName = "INT";
      DbDataReaderGetValueMethodName = "GetUInt32";
    }

  }
}