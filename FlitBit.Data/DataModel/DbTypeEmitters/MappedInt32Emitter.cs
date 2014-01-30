using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
	internal class MappedInt32Emitter : MappedDbTypeEmitter<int, DbType>
	{
		internal MappedInt32Emitter()
			: base(DbType.Int32, DbType.Int32)
		{
			this.SpecializedSqlTypeName = "INT";
		  DbDataReaderGetValueMethodName = "GetInt32";
		}

	}
}