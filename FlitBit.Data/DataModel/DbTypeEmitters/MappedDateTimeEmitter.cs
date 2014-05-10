#region COPYRIGHTę 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
  internal class MappedDateTimeEmitter : MappedDbTypeEmitter<DateTime, DbType>
  {
    internal MappedDateTimeEmitter()
      : base(DbType.DateTime, DbType.DateTime) { DbDataReaderGetValueMethodName = "GetDateTime"; }
  }
}