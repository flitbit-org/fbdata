using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedInt64Emitter : MappedDbTypeEmitter<long, DbType>
	{
		internal MappedInt64Emitter()
			: base(DbType.Int64, DbType.Int64)
		{
		  DbDataReaderGetValueMethodName = "GetInt64";
		}


	}
}