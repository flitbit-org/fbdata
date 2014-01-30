using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedCharEmitter : MappedDbTypeEmitter<char, DbType>
	{
		internal MappedCharEmitter(DbType dbType)
			: base(dbType, dbType)
		{
		  DbDataReaderGetValueMethodName = "GetChar";
		}

	}
}