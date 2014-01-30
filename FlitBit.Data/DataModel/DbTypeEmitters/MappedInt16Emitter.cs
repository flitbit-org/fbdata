using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedInt16Emitter : MappedDbTypeEmitter<short, DbType>
	{
		internal MappedInt16Emitter()
			: base(DbType.Int16, DbType.Int16)
		{
			this.SpecializedSqlTypeName = "SMALLINT";
		  DbDataReaderGetValueMethodName = "GetInt16";
		}

	}
}