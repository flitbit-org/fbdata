using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedNullableSingleEmitter : SqlDbTypeEmitter<float?>
  {
    internal SqlMappedNullableSingleEmitter()
      : base(DbType.Single, SqlDbType.Real, typeof(float))
    {
    }
    public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
    {
      var il = method.GetILGenerator();
      reader.LoadValue(il);
      columnIndex.LoadValue(il);
      il.CallVirtual<DbDataReader>("GetDouble", typeof(int));
      EmitTranslateDbType(il);
    }

    protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    {
      il.LoadLocalAddress(local);
      il.CallVirtual<float?>("get_Value");
      il.NewObj(typeof(SqlSingle).GetConstructor(new[] { typeof(float) }));
      il.Box(typeof(SqlSingle));
    }


    protected override void EmitTranslateDbType(ILGenerator il)
    {
      il.NewObj(typeof(float?).GetConstructor(new[] { typeof(float) }));
    }
  }
}