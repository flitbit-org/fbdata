#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedGuidEmitter : MappedDbTypeEmitter<Guid, DbType>
	{
		internal MappedGuidEmitter()
			: base(DbType.Guid, DbType.Guid)
		{
		  DbDataReaderGetValueMethodName = "GetGuid";
		}
	}
}