#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
  internal class MappedInt64Emitter : MappedDbTypeEmitter<long, DbType>
  {
    internal MappedInt64Emitter()
      : base(DbType.Int64, DbType.Int64) { DbDataReaderGetValueMethodName = "GetInt64"; }
  }
}