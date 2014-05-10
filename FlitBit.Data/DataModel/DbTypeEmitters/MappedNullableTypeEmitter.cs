#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel.DbTypeEmitters
{
  internal abstract class MappedNullableTypeEmitter<TBaseType, TDbType> : MappedDbTypeEmitter<TBaseType?, TDbType>
    where TBaseType: struct
    where TDbType: struct
  {
    internal MappedNullableTypeEmitter(DbType dbType, TDbType specializedDbType)
      : base(dbType, specializedDbType, typeof(TBaseType))
    {}

    protected internal override void EmitDbParameterSetValue(ILGenerator il, ColumnMapping column, LocalBuilder parm,
      LocalBuilder local, LocalBuilder flag)
    {
      // if (field.HasValue) {

      var gotoSetNull = il.DefineLabel();
      var fin = il.DefineLabel();

      il.LoadLocalAddress(local);
      il.Call<TBaseType?>("get_HasValue");
      il.Load_I4_0();
      il.CompareEqual();
      il.StoreLocal(flag);
      il.LoadLocal(flag);
      il.BranchIfTrue(gotoSetNull);

      //   param.Value = field.Value;

      il.LoadLocal(parm);
      EmitTranslateRuntimeType(il, local);
      EmitInvokeDbParameterSetValue(il);
      il.Branch(fin);

      // } else {
      //   param.Value = DbNull.Value;

      il.MarkLabel(gotoSetNull);
      il.LoadLocal(parm);
      il.LoadField(typeof(DBNull).GetField("Value", BindingFlags.Static | BindingFlags.Public));
      EmitInvokeDbParameterSetValue(il);

      // }

      il.MarkLabel(fin);
    }

    protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocalAddress(local);
      il.Call<TBaseType?>("get_Value");
      il.Box(typeof(TBaseType));
    }

    protected override void EmitTranslateDbType(ILGenerator il)
    {
      il.NewObj(typeof(TBaseType?).GetConstructor(new[] { typeof(TBaseType) }));
    }
  }
}