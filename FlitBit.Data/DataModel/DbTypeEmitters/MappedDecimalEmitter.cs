using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedDecimalEmitter : MappedDbTypeEmitter<decimal, DbType>
	{
		internal MappedDecimalEmitter()
			: base(DbType.Decimal, DbType.Decimal)
		{
		  DbDataReaderGetValueMethodName = "GetDecimal";
		}

	}
}