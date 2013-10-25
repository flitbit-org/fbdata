namespace FlitBit.Data.DataModel
{
	/// <summary>
	/// Interface for querying models without criteria.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	public interface IDataModelQueryManyCommand<out TModel, in TDbConnection>
	{
		/// <summary>
		/// Executes the query on the specified connection according to the specified behavior (possibly paging).
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="behavior">behaviors, possibly paging</param>
		/// <returns>a data model query result</returns>
		IDataModelQueryResult<TModel> ExecuteMany(IDbContext cx, TDbConnection cn, QueryBehavior behavior);
	}

	/// <summary>
	/// Interface for querying models with criteria.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	/// <typeparam name="TCriteria"></typeparam>
	public interface IDataModelQueryManyCommand<out TModel, in TDbConnection, in TCriteria>
	{
		/// <summary>
		/// Executes the query using the specified criteria, on the specified connection, according to the specified behavior (possibly paging).
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="behavior">behaviors, possibly paging</param>
		/// <param name="criteria">criteria used to bind the command.</param>
		/// <returns>a data model query result</returns>
		IDataModelQueryResult<TModel> ExecuteMany(IDbContext cx, TDbConnection cn, QueryBehavior behavior, TCriteria criteria);
	}
}