using System;
using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedDateTimeEmitter : MappedDbTypeEmitter<DateTime, DbType>
	{
		internal MappedDateTimeEmitter()
			: base(DbType.DateTime, DbType.DateTime)
		{
		  DbDataReaderGetValueMethodName = "GetDateTime";
		}

	}
}