using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using FlitBit.Data.Meta;
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
			il.CallVirtual<SqlParameter>("set_SqlDbType", typeof(SqlDbType));
		}

		protected internal override void EmitDbParameterSetValue(ILGenerator il, ColumnMapping column, LocalBuilder parm,
			LocalBuilder local,
			LocalBuilder flag)
		{
			il.LoadLocal(parm);
			il.LoadLocal(local);
			EmitTranslateRuntimeType(il);
			il.CallVirtual<SqlParameter>("set_SqlValue", typeof(object));
		}
	}
}