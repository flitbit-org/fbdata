#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
  internal class MappedSingleEmitter : MappedDbTypeEmitter<float, DbType>
  {
    internal MappedSingleEmitter()
      : base(DbType.Single, DbType.Single) { DbDataReaderGetValueMethodName = "GetFloat"; }
  }
}