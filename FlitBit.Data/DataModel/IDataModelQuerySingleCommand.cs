namespace FlitBit.Data.DataModel
{
	/// <summary>
	/// Interface for querying a single model without criteria.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	public interface IDataModelQuerySingleCommand<out TModel, in TDbConnection>
	{
		/// <summary>
		/// Executes the query on the specified connection.
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <returns>zero or one model (implicitly single-or-default)</returns>
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn);
	}

	/// <summary>
	/// Interface for querying a single model by specified criteria.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	/// <typeparam name="TCriteria"></typeparam>
	public interface IDataModelQuerySingleCommand<out TModel, in TDbConnection, in TCriteria>
	{
		/// <summary>
		/// Executes the query on the specified connection.
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="criteria">the criteria used to bind the command</param>
		/// <returns>zero or one model (implicitly single-or-default)</returns>
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn, TCriteria criteria);
	}
}