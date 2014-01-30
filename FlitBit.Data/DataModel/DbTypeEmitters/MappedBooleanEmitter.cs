using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedBooleanEmitter : MappedDbTypeEmitter<bool, DbType>
	{
		internal MappedBooleanEmitter()
			: base(DbType.Boolean, DbType.Boolean)
		{
		  DbDataReaderGetValueMethodName = "GetBoolean";
		}

	}
}