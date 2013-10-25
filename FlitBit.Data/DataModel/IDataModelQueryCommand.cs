namespace FlitBit.Data.DataModel
{
	public interface IDataModelQueryCommand<out TModel, in TDbConnection, in TCriteria> 
		: IDataModelQuerySingleCommand<TModel, TDbConnection, TCriteria>,
		IDataModelQueryManyCommand<TModel, TDbConnection, TCriteria>
	{
	}
}