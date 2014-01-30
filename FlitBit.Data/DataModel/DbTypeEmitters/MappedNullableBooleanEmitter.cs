using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
  internal class MappedNullableBooleanEmitter : MappedNullableTypeEmitter<bool, DbType>
	{
		internal MappedNullableBooleanEmitter()
			: base(DbType.Boolean, DbType.Boolean)
		{
		  DbDataReaderGetValueMethodName = "GetBoolean";
		}

	}
}