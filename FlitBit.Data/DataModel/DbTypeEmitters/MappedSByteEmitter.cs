using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedSByteEmitter : MappedDbTypeEmitter<sbyte, DbType>
	{
		internal MappedSByteEmitter()
			: base(DbType.SByte, DbType.SByte)
		{
		  DbDataReaderGetValueMethodName = "GetSByte";
		}
	}
}