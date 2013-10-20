using System.Data;
using System.Data.SqlClient;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
	internal abstract class SqlDbTypeEmitter<T> : MappedDbTypeEmitter<T, SqlDbType>
	{
		internal SqlDbTypeEmitter(DbType dbType, SqlDbType sqlDbType)
			: base(dbType, sqlDbType)
		{
		}

		internal protected override void EmitDbParameterSetDbType(ILGenerator il, LocalBuilder parm)
		{
			il.LoadLocal(parm);
			il.LoadValue(this.SpecializedDbTypeValue);
			il.CallVirtual<SqlParameter>("set_SqlDbType");
		}
	}
}