using System.Data.Common;
using System.Reflection;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.SqlServer
{
    public class SqlDataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> 
        DataModelUpdateCommandBuilder<>: IDataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> where TDbConnection : DbConnection
    {
        public IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> Where(System.Linq.Expressions.Expression<System.Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, bool>> predicate)
        {
            throw new System.NotImplementedException();
        }
    }
}
