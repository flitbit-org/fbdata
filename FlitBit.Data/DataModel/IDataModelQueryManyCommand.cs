namespace FlitBit.Data.DataModel
{
	public interface IDataModelQueryManyCommand<out TModel, in TDbConnection>
	{
		IDataModelQueryResult<TModel> ExecuteMany(IDbContext cx, TDbConnection cn, QueryBehavior behavior);
	}

	public interface IDataModelQueryManyCommand<out TModel, in TDbConnection, in TKey>
	{
		IDataModelQueryResult<TModel> ExecuteMany(IDbContext cx, TDbConnection cn, QueryBehavior behavior, TKey key);
	}
}