using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Reflection.Emit;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal class SqlMappedStringEmitter : SqlMappedAnyToStringEmitter<string>
	{
		internal SqlMappedStringEmitter(SqlDbType dbType)
			: base(dbType)
		{ }

	}
}
