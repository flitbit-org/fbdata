using System.Data;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedStringEmitter : SqlMappedAnyToStringEmitter<string>
	{
		internal SqlMappedStringEmitter(SqlDbType dbType)
			: base(dbType)
		{}
	}
}
