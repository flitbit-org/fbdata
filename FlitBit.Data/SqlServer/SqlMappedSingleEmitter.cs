#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;

namespace FlitBit.Data.SqlServer
{
    internal class SqlMappedSingleEmitter : SqlDbTypeEmitter<float>
    {
        internal SqlMappedSingleEmitter()
            : base(DbType.Single, SqlDbType.Real) { DbDataReaderGetValueMethodName = "GetFloat"; }

        public override void EmitColumnInitializationDDL<TModel>(SqlWriter buffer, IMapping<TModel> mapping,
            ColumnMapping<TModel> col) { }
    }
}