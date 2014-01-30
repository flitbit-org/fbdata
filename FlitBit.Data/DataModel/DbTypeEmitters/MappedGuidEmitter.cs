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