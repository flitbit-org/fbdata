using System;
using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedDateTimeOffsetEmitter : MappedDbTypeEmitter<DateTimeOffset, DbType>
	{
		internal MappedDateTimeOffsetEmitter()
			: base(DbType.DateTime, DbType.DateTime)
		{
		  DbDataReaderGetValueMethodName = "GetDateTimeOffset";
		}

	}
}