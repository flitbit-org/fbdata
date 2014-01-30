using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedNullableBoolAsBitEmitter : SqlDbTypeNullableEmitter<bool>
	{
		internal SqlMappedNullableBoolAsBitEmitter()
			: base(DbType.Boolean, SqlDbType.Bit)
		{
		  DbDataReaderGetValueMethodName = "GetBoolean";
		}

		protected override string TransformConstantValueToString(object value)
		{
			return (bool)value ? "1" : "0";
		}
	}
}