#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using FlitBit.Data.DataModel;
using FlitBit.Data.DataModel.DbTypeEmitters;
using FlitBit.Emit;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Emit;

namespace FlitBit.Data.SqlServer
{
	internal abstract class SqlDbTypeEmitter<T> : MappedDbTypeEmitter<T, SqlDbType>
	{
		internal SqlDbTypeEmitter(DbType dbType, SqlDbType sqlDbType)
			: this(dbType, sqlDbType, typeof (T))
		{
		}

		internal SqlDbTypeEmitter(DbType dbType, SqlDbType sqlDbType, Type underlying)
			: base(dbType, sqlDbType, underlying)
		{
		}

		internal protected override void EmitDbParameterSetDbType(ILGenerator il, LocalBuilder parm)
		{
			il.LoadLocal(parm);
			il.LoadValue(this.SpecializedDbTypeValue);
			il.CallVirtual<SqlParameter>("set_SqlDbType", typeof(SqlDbType));
		}
	}

  internal abstract class SqlDbTypeNullableEmitter<T> : MappedNullableTypeEmitter<T, SqlDbType>
    where T: struct
  {
    internal SqlDbTypeNullableEmitter(DbType dbType, SqlDbType sqlDbType)
      : base(dbType, sqlDbType)
    {
    }
    
    internal protected override void EmitDbParameterSetDbType(ILGenerator il, LocalBuilder parm)
    {
      il.LoadLocal(parm);
      il.LoadValue(this.SpecializedDbTypeValue);
      il.CallVirtual<SqlParameter>("set_SqlDbType", typeof(SqlDbType));
    }
  }
}