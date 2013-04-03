using System.Data.Common;

namespace FlitBit.Data.DataModel
{		
	public interface IDataModelNonQueryCommand<out TModel, in TDbConnection>
		where TDbConnection : DbConnection
	{
		int Execute(IDbContext cx, TDbConnection cn);
	}

	public interface IDataModelNonQueryCommand<out TModel, in TDbConnection, in TKey>
		where TDbConnection : DbConnection
	{
		int Execute(IDbContext cx, TDbConnection cn, TKey key);
	}

	public interface IDataModelNonQueryCommand<out TModel, in TDbConnection, in TItem1, in TItem2>
		where TDbConnection : DbConnection
	{
		int Execute(IDbContext cx, TDbConnection cn, TItem1 item1, TItem2 item2);
	}
}