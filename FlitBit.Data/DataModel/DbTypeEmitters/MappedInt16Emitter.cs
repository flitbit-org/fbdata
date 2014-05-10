#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
  internal class MappedInt16Emitter : MappedDbTypeEmitter<short, DbType>
  {
    internal MappedInt16Emitter()
      : base(DbType.Int16, DbType.Int16)
    {
      this.SpecializedSqlTypeName = "SMALLINT";
      DbDataReaderGetValueMethodName = "GetInt16";
    }
  }
}