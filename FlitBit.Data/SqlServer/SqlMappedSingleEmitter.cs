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
  internal class SqlMappedSingleEmitter : SqlDbTypeEmitter<float>
  {
    internal SqlMappedSingleEmitter()
      : base(DbType.Single, SqlDbType.Real)
    {
      DbDataReaderGetValueMethodName = "GetFloat";
    }

    public override void EmitColumnInitializationDDL<TModel>(StringBuilder buffer, IMapping<TModel> mapping, ColumnMapping<TModel> col)
    {
    }

  }
}