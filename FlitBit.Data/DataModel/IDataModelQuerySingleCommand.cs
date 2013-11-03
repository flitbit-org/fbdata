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
	/// <typeparam name="TParam"></typeparam>
	public interface IDataModelQuerySingleCommand<out TModel, in TDbConnection, in TParam>
	{
		/// <summary>
		/// Executes the query on the specified connection.
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="param"></param>
		/// <returns>zero or one model (implicitly single-or-default)</returns>
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn, TParam param);
	}
	/// <summary>
	/// Interface for querying a single model by specified criteria.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	public interface IDataModelQuerySingleCommand<out TModel, in TDbConnection, in TParam, in TParam1>
	{
		/// <summary>
		/// Executes the query on the specified connection.
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="arg0"></param>
		/// <param name="arg1"></param>
		/// <returns>zero or one model (implicitly single-or-default)</returns>
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn, TParam arg0, TParam1 arg1);
	}
	/// <summary>
	/// Interface for querying a single model by specified criteria.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	public interface IDataModelQuerySingleCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2>
	{
		/// <summary>
		/// Executes the query on the specified connection.
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="arg0"></param>
		/// <param name="arg1"></param>
		/// <param name="arg2"></param>
		/// <returns>zero or one model (implicitly single-or-default)</returns>
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn, TParam arg0, TParam1 arg1, TParam2 arg2);
	}
	/// <summary>
	/// Interface for querying a single model by specified criteria.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	public interface IDataModelQuerySingleCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2, in TParam3>
	{
		/// <summary>
		/// Executes the query on the specified connection.
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="arg0"></param>
		/// <param name="arg1"></param>
		/// <param name="arg2"></param>
		/// <param name="arg3"></param>
		/// <returns>zero or one model (implicitly single-or-default)</returns>
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn, TParam arg0, TParam1 arg1, TParam2 arg2, TParam3 arg3);
	}
	/// <summary>
	/// Interface for querying a single model by specified criteria.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	public interface IDataModelQuerySingleCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2, in TParam3, in TParam4>
	{
		/// <summary>
		/// Executes the query on the specified connection.
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="arg0"></param>
		/// <param name="arg1"></param>
		/// <param name="arg2"></param>
		/// <param name="arg3"></param>
		/// <param name="arg4"></param>
		/// <returns>zero or one model (implicitly single-or-default)</returns>
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn, TParam arg0, TParam1 arg1, TParam2 arg2, TParam3 arg3, TParam4 arg4);
	}
	/// <summary>
	/// Interface for querying a single model by specified criteria.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	public interface IDataModelQuerySingleCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2, in TParam3, in TParam4, in TParam5>
	{
		/// <summary>
		/// Executes the query on the specified connection.
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="arg0"></param>
		/// <param name="arg1"></param>
		/// <param name="arg2"></param>
		/// <param name="arg3"></param>
		/// <param name="arg4"></param>
		/// <param name="arg5"></param>
		/// <returns>zero or one model (implicitly single-or-default)</returns>
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn, TParam arg0, TParam1 arg1, TParam2 arg2, TParam3 arg3, TParam4 arg4,
			TParam5 arg5);
	}
	/// <summary>
	/// Interface for querying a single model by specified criteria.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	/// <typeparam name="TParam6"></typeparam>
	public interface IDataModelQuerySingleCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6>
	{
		/// <summary>
		/// Executes the query on the specified connection.
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="arg0"></param>
		/// <param name="arg1"></param>
		/// <param name="arg2"></param>
		/// <param name="arg3"></param>
		/// <param name="arg4"></param>
		/// <param name="arg5"></param>
		/// <param name="arg6"></param>
		/// <returns>zero or one model (implicitly single-or-default)</returns>
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn, TParam arg0, TParam1 arg1, TParam2 arg2, TParam3 arg3, TParam4 arg4,
			TParam5 arg5, TParam6 arg6);
	}
	/// <summary>
	/// Interface for querying a single model by specified criteria.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	/// <typeparam name="TParam6"></typeparam>
	/// <typeparam name="TParam7"></typeparam>
	public interface IDataModelQuerySingleCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7>
	{
		/// <summary>
		/// Executes the query on the specified connection.
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="arg0"></param>
		/// <param name="arg1"></param>
		/// <param name="arg2"></param>
		/// <param name="arg3"></param>
		/// <param name="arg4"></param>
		/// <param name="arg5"></param>
		/// <param name="arg6"></param>
		/// <param name="arg7"></param>
		/// <returns>zero or one model (implicitly single-or-default)</returns>
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn, TParam arg0, TParam1 arg1, TParam2 arg2, TParam3 arg3, TParam4 arg4,
			TParam5 arg5, TParam6 arg6, TParam7 arg7);
	}
	/// <summary>
	/// Interface for querying a single model by specified criteria.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	/// <typeparam name="TParam6"></typeparam>
	/// <typeparam name="TParam7"></typeparam>
	/// <typeparam name="TParam8"></typeparam>
	public interface IDataModelQuerySingleCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, in TParam8>
	{
		/// <summary>
		/// Executes the query on the specified connection.
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="arg0"></param>
		/// <param name="arg1"></param>
		/// <param name="arg2"></param>
		/// <param name="arg3"></param>
		/// <param name="arg4"></param>
		/// <param name="arg5"></param>
		/// <param name="arg6"></param>
		/// <param name="arg7"></param>
		/// <param name="arg8"></param>
		/// <returns>zero or one model (implicitly single-or-default)</returns>
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn, TParam arg0, TParam1 arg1, TParam2 arg2, TParam3 arg3, TParam4 arg4,
			TParam5 arg5, TParam6 arg6, TParam7 arg7, TParam8 arg8);
	}
	/// <summary>
	/// Interface for querying a single model by specified criteria.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	/// <typeparam name="TParam"></typeparam>
	/// <typeparam name="TParam1"></typeparam>
	/// <typeparam name="TParam2"></typeparam>
	/// <typeparam name="TParam3"></typeparam>
	/// <typeparam name="TParam4"></typeparam>
	/// <typeparam name="TParam5"></typeparam>
	/// <typeparam name="TParam6"></typeparam>
	/// <typeparam name="TParam7"></typeparam>
	/// <typeparam name="TParam8"></typeparam>
	/// <typeparam name="TParam9"></typeparam>
	public interface IDataModelQuerySingleCommand<out TModel, in TDbConnection, in TParam, in TParam1, in TParam2, in TParam3, in TParam4, in TParam5, in TParam6, in TParam7, in TParam8, in TParam9>
	{
		/// <summary>
		/// Executes the query on the specified connection.
		/// </summary>
		/// <param name="cx">the db context</param>
		/// <param name="cn">a db connection used to execute the command</param>
		/// <param name="arg0"></param>
		/// <param name="arg1"></param>
		/// <param name="arg2"></param>
		/// <param name="arg3"></param>
		/// <param name="arg4"></param>
		/// <param name="arg5"></param>
		/// <param name="arg6"></param>
		/// <param name="arg7"></param>
		/// <param name="arg8"></param>
		/// <param name="arg9"></param>
		/// <returns>zero or one model (implicitly single-or-default)</returns>
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn, TParam arg0, TParam1 arg1, TParam2 arg2, TParam3 arg3, TParam4 arg4,
			TParam5 arg5, TParam6 arg6, TParam7 arg7, TParam8 arg8, TParam9 arg9);
	}
}