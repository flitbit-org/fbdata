#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedTypeToStringEmitter : SqlMappedAnyToStringEmitter<Type>
  {
    public SqlMappedTypeToStringEmitter()
      : base(SqlDbType.NVarChar, typeof(string))
    {}

    protected override void EmitTranslateDbType(ILGenerator il)
    {
      il.Call<Type>("GetType", BindingFlags.Static | BindingFlags.Public, typeof(string));
    }

    /// <summary>
    ///   Emits IL to translate the runtime type to the dbtype.
    /// </summary>
    /// <param name="il"></param>
    /// <param name="local"></param>
    /// <remarks>
    ///   It is the responsibility of this method to ensure the local is loaded,
    ///   translated, and on the top of the stack.
    /// </remarks>
    protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocal(local);
      il.CallVirtual<Type>("get_FullName");
    }

    public override DbTypeDetails GetDbTypeDetails(ColumnMapping column)
    {
      Debug.Assert(column.Member.DeclaringType != null, "column.Member.DeclaringType != null");
      var bindingName = String.Concat(column.Member.DeclaringType.Name, "_", column.Member.Name);
      return new DbTypeDetails(column.Member.Name, bindingName, 400, null, null);
    }
  }
}