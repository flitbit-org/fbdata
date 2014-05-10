#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;

namespace FlitBit.Data.SqlServer
{
  internal class SqlMappedDoubleEmitter : SqlDbTypeEmitter<double>
  {
    internal SqlMappedDoubleEmitter()
      : base(DbType.Double, SqlDbType.Float)
    {
      DbDataReaderGetValueMethodName = "GetDouble";
    }

    public override void EmitColumnInitializationDDL<TModel>(StringBuilder buffer, IMapping<TModel> mapping,
      ColumnMapping<TModel> col)
    {}

    //protected override void EmitTranslateRuntimeType(ILGenerator il, LocalBuilder local)
    //{
    //  il.LoadLocal(local);
    //  il.NewObj(typeof(SqlDouble).GetConstructor(new[] { typeof(double) }));
    //  il.Box(typeof(SqlDouble));
    //}
  }
}