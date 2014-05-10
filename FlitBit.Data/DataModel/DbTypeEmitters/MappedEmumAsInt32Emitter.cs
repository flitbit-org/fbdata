#region COPYRIGHTę 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
  internal class MappedEmumAsInt32Emitter<TEnum> : MappedDbTypeEmitter<TEnum, DbType>
    where TEnum : struct
  {
    internal MappedEmumAsInt32Emitter()
      : base(DbType.Int32, DbType.Int32)
    {
      this.SpecializedSqlTypeName = "INT";
      DbDataReaderGetValueMethodName = "GetInt32";
    }

    protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocal(local);
      il.Call(typeof(Convert).GetMethod("ToInt32", BindingFlags.Public | BindingFlags.Static, null, new[]
      {
        typeof(object)
      }, null));
    }
  }
}