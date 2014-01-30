using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{																																	
	internal class MappedDoubleEmitter : MappedDbTypeEmitter<double, DbType>
	{
		internal MappedDoubleEmitter()
			: base(DbType.Double, DbType.Double)
		{
		  DbDataReaderGetValueMethodName = "GetDouble";
		}

	}
}