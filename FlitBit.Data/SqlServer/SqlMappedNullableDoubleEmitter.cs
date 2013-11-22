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

    /// <summary>
    ///   Emits IL to translate the runtime type to the dbtype.
    /// </summary>
    /// <param name="il"></param>
    /// <remarks>
    ///   At the time of the call the runtime value is on top of the stack.
    ///   When the method returns the translated type must be on the top of the stack.
    /// </remarks>
    protected override void EmitTranslateRuntimeType(ILGenerator il)
    {
      il.NewObj(typeof(SqlDouble).GetConstructor(new[] { typeof(double) }));
      il.Box(typeof(SqlDouble));
    }


    protected override void EmitTranslateDbType(ILGenerator il)
    {
      il.NewObj(typeof(double?).GetConstructor(new[] { typeof(double) }));
    }
  }
}