#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
  internal class MappedUInt64Emitter : MappedDbTypeEmitter<uint, DbType>
  {
    internal MappedUInt64Emitter()
      : base(DbType.UInt64, DbType.UInt64) { DbDataReaderGetValueMethodName = "GetUInt64"; }
  }
}