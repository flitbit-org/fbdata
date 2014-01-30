using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedNullableDoubleEmitter : SqlDbTypeEmitter<double?>
  {
    internal SqlMappedNullableDoubleEmitter()
      : base(DbType.Double, SqlDbType.Float, typeof(double))
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
      il.CallVirtual<double?>("get_Value");
      il.NewObj(typeof(SqlDouble).GetConstructor(new[] { typeof(double) }));
      il.Box(typeof(SqlDouble));
    }


    protected override void EmitTranslateDbType(ILGenerator il)
    {
      il.NewObj(typeof(double?).GetConstructor(new[] { typeof(double) }));
    }
  }
}