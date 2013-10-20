namespace FlitBit.Data.DataModel
{
	public interface IDataModelQuerySingleCommand<out TModel, in TDbConnection, in TKey>
	{
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn, TKey model);
	}

	public interface IDataModelQuerySingleCommand<out TModel, in TDbConnection>
	{
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn);
	}
}