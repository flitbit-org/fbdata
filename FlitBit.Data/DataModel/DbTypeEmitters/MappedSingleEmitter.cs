using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedSingleEmitter : MappedDbTypeEmitter<float, DbType>
	{
		internal MappedSingleEmitter()
			: base(DbType.Single, DbType.Single)
		{
		  DbDataReaderGetValueMethodName = "GetFloat";
		}

	}
}