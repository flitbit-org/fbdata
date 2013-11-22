using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection.Emit;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedDoubleEmitter : SqlDbTypeEmitter<double>
  {
    internal SqlMappedDoubleEmitter()
      : base(DbType.Double, SqlDbType.Float)
    {
    }
    public override void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex, DbTypeDetails details)
    {
      var il = method.GetILGenerator();
      reader.LoadValue(il);
      columnIndex.LoadValue(il);
      il.CallVirtual<DbDataReader>("GetDouble", typeof(int));
    }
    public override void EmitColumnInitializationDDL<TModel>(StringBuilder buffer, IMapping<TModel> mapping, ColumnMapping<TModel> col)
    {
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
  }
}