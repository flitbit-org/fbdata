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
      DbDataReaderGetValueMethodName = "GetDouble";
    }
    
    public override void EmitColumnInitializationDDL<TModel>(StringBuilder buffer, IMapping<TModel> mapping, ColumnMapping<TModel> col)
    {
    }

    //protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    //{
    //  il.LoadLocal(local);
    //  il.NewObj(typeof(SqlDouble).GetConstructor(new[] { typeof(double) }));
    //  il.Box(typeof(SqlDouble));
    //}
  }
}