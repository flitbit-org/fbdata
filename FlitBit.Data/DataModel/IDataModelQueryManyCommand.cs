namespace FlitBit.Data.DataModel
{
	public interface IDataModelQueryManyCommand<out TModel, in TDbConnection>
	{
		IDataModelQueryResult<TModel> ExecuteMany(IDbContext cx, TDbConnection cn, QueryBehavior behavior);
	}

	public interface IDataModelQueryManyCommand<out TModel, in TDbConnection, in TCriteria>
	{
		IDataModelQueryResult<TModel> ExecuteMany(IDbContext cx, TDbConnection cn, QueryBehavior behavior, TCriteria key);
	}
}