using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedUInt16Emitter : MappedDbTypeEmitter<uint, DbType>
	{
		internal MappedUInt16Emitter()
			: base(DbType.UInt16, DbType.UInt16)
		{
			this.SpecializedSqlTypeName = "SMALLINT";
		  DbDataReaderGetValueMethodName = "GetUInt16";
		}
	}
}