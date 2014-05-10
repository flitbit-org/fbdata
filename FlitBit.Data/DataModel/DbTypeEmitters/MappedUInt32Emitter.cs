#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

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