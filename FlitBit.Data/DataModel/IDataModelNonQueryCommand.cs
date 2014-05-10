#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System.Data.Common;

namespace FlitBit.Data.DataModel
{		
	public interface IDataModelNonQueryCommand<out TModel, in TDbConnection>
		where TDbConnection : DbConnection
	{
		int Execute(IDbContext cx, TDbConnection cn);
	}
	public interface IDataModelNonQueryCommand<out TModel, in TDbConnection, in TParam>
		where TDbConnection : DbConnection
	{
		int Execute(IDbContext cx, TDbConnection cn, TParam param);
	}

	public interface IDataModelNonQueryCommand<out TModel, in TDbConnection, in TParam, in TParam1>
		where TDbConnection : DbConnection
	{
		int Execute(IDbContext cx, TDbConnection cn, TParam param, TParam1 param1);
	}
    public interface IDataModelNonQueryCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2>
            where TDbConnection : DbConnection
    {
        int Execute(IDbContext cx, TDbConnection cn, TParam param, TParam1 param1, TParam2 param2);
    }
    public interface IDataModelNonQueryCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2, in TParam3>
            where TDbConnection : DbConnection
    {
        int Execute(IDbContext cx, TDbConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3);
    }
    public interface IDataModelNonQueryCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2, in TParam3, in TParam4>
            where TDbConnection : DbConnection
    {
        int Execute(IDbContext cx, TDbConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4);
    }
    public interface IDataModelNonQueryCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2, in TParam3, in TParam4, in TParam5>
            where TDbConnection : DbConnection
    {
        int Execute(IDbContext cx, TDbConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5);
    }
    public interface IDataModelNonQueryCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6>
            where TDbConnection : DbConnection
    {
        int Execute(IDbContext cx, TDbConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6);
    }
    public interface IDataModelNonQueryCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7>
        where TDbConnection : DbConnection
    {
        int Execute(IDbContext cx, TDbConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7);
    }
    public interface IDataModelNonQueryCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, in TParam8>
        where TDbConnection : DbConnection
    {
        int Execute(IDbContext cx, TDbConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8);
    }

    public interface IDataModelNonQueryCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, in TParam8, in TParam9>
        where TDbConnection : DbConnection
    {
        int Execute(IDbContext cx, TDbConnection cn, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9);
    }
 
}