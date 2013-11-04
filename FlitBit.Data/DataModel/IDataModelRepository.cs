using System;
using System.Data.Common;
using System.Linq.Expressions;

namespace FlitBit.Data.DataModel
{
	/// <summary>
	/// Repository over a data model.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TIdentityKey"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	public interface IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> : IDataRepository<TDataModel, TIdentityKey>
		where TDbConnection: DbConnection
	{
		/// <summary>
		/// Gets the data model's binder.
		/// </summary>
		IDataModelBinder<TDataModel, TIdentityKey, TDbConnection> Binder { get; }

		/// <summary>
		/// Creates a query command for the criteria specified.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="predicate">a predicate expression</param>
		/// <returns></returns>
		IDataModelQueryCommand<TDataModel, TDbConnection, TParam> Where<TParam>(string queryKey,
			Expression<Func<TDataModel, TParam, bool>> predicate);

		/// <summary>
		/// Creates a query command for the criteria specified.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="predicate">a predicate expression</param>
		/// <returns></returns>
		IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1> Where<TParam, TParam1>(string queryKey,
			Expression<Func<TDataModel, TParam, TParam1, bool>> predicate);

		/// <summary>
		/// Creates a query command for the criteria specified.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="predicate">a predicate expression</param>
		/// <returns></returns>
		IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> Where<TParam, TParam1, TParam2>(string queryKey,
			Expression<Func<TDataModel, TParam, TParam1, TParam2, bool>> predicate);

		/// <summary>
		/// Creates a query command for the criteria specified.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="predicate">a predicate expression</param>
		/// <returns></returns>
		IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> Where<TParam, TParam1, TParam2, TParam3>(string queryKey,
			Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, bool>> predicate);

		/// <summary>
		/// Creates a query command for the criteria specified.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="predicate">a predicate expression</param>
		/// <returns></returns>
		IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> Where<TParam, TParam1, TParam2, TParam3, TParam4>(string queryKey,
			Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, bool>> predicate);

		/// <summary>
		/// Creates a query command for the criteria specified.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="predicate">a predicate expression</param>
		/// <returns></returns>
		IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(string queryKey,
			Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, bool>> predicate);

		/// <summary>
		/// Creates a query command for the criteria specified.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="predicate">a predicate expression</param>
		/// <returns></returns>
		IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(string queryKey,
			Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool>> predicate);

		/// <summary>
		/// Creates a query command for the criteria specified.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="predicate">a predicate expression</param>
		/// <returns></returns>
		IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(string queryKey,
			Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool>> predicate);

		/// <summary>
		/// Creates a query command for the criteria specified.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="predicate">a predicate expression</param>
		/// <returns></returns>
		IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(string queryKey,
			Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, bool>> predicate);

		/// <summary>
		/// Creates a query command for the criteria specified.
		/// </summary>
		/// <param name="queryKey"></param>
		/// <param name="predicate">a predicate expression</param>
		/// <returns></returns>
		IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> Where<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(string queryKey,
			Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, bool>> predicate);

		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning a single data model instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="param"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <returns></returns>
		TDataModel ExecuteSingle<TParam>(
			IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam> cmd,
			IDbContext cx, TParam param);
	
		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning a single data model instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <returns></returns>
		TDataModel ExecuteSingle<TParam, TParam1>(
			IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam, TParam1> cmd,
			IDbContext cx, TParam param, TParam1 param1);
	
		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning a single data model instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <returns></returns>
		TDataModel ExecuteSingle<TParam, TParam1, TParam2>(
			IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> cmd,
			IDbContext cx, TParam param, TParam1 param1, TParam2 param2);
	
		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning a single data model instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <returns></returns>
		TDataModel ExecuteSingle<TParam, TParam1, TParam2, TParam3>(
			IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> cmd,
			IDbContext cx, TParam param, TParam1 param1, TParam2 param2, TParam3 param3);
	
		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning a single data model instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <returns></returns>
		TDataModel ExecuteSingle<TParam, TParam1, TParam2, TParam3, TParam4>(
			IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> cmd,
			IDbContext cx, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4);
	
		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning a single data model instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <typeparam name="TParam5"></typeparam>
		/// <returns></returns>
		TDataModel ExecuteSingle<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(
			IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> cmd,
			IDbContext cx, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5);
	
		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning a single data model instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <typeparam name="TParam5"></typeparam>
		/// <typeparam name="TParam6"></typeparam>
		/// <returns></returns>
		TDataModel ExecuteSingle<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
			IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> cmd,
			IDbContext cx, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6);
	
		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning a single data model instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <typeparam name="TParam5"></typeparam>
		/// <typeparam name="TParam6"></typeparam>
		/// <typeparam name="TParam7"></typeparam>
		/// <returns></returns>
		TDataModel ExecuteSingle<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
			IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> cmd,
			IDbContext cx, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7);
	
		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning a single data model instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <param name="param8"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <typeparam name="TParam5"></typeparam>
		/// <typeparam name="TParam6"></typeparam>
		/// <typeparam name="TParam7"></typeparam>
		/// <typeparam name="TParam8"></typeparam>
		/// <returns></returns>
		TDataModel ExecuteSingle<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
			IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> cmd,
			IDbContext cx, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8);
	
		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning a single data model instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <param name="param8"></param>
		/// <param name="param9"></param>
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
		/// <returns></returns>
		TDataModel ExecuteSingle<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(IDataModelQuerySingleCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> cmd,
			IDbContext cx, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9);

		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning data model results.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="behavior"></param>
		/// <param name="param"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <returns></returns>
		IDataModelQueryResult<TDataModel> ExecuteMany<TParam>(
			IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam> cmd,
			IDbContext cx, QueryBehavior behavior, TParam param);

		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning data model results.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="behavior"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <returns></returns>
		IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1>(
			IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam, TParam1> cmd,
			IDbContext cx, QueryBehavior behavior, TParam param, TParam1 param1);

		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning data model results.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="behavior"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <returns></returns>
		IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1, TParam2>(
			IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> cmd,
			IDbContext cx, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2);

		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning data model results.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="behavior"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <returns></returns>
		IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1, TParam2, TParam3>(
			IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> cmd,
			IDbContext cx, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2, TParam3 param3);

		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning data model results.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="behavior"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <returns></returns>
		IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1, TParam2, TParam3, TParam4>(
			IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> cmd,
			IDbContext cx, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4);

		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning data model results.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="behavior"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <typeparam name="TParam5"></typeparam>
		/// <returns></returns>
		IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1, TParam2, TParam3, TParam4, TParam5>(
			IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> cmd,
			IDbContext cx, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5);

		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning data model results.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="behavior"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <typeparam name="TParam5"></typeparam>
		/// <typeparam name="TParam6"></typeparam>
		/// <returns></returns>
		IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
			IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> cmd,
			IDbContext cx, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6);

		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning data model results.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="behavior"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <typeparam name="TParam5"></typeparam>
		/// <typeparam name="TParam6"></typeparam>
		/// <typeparam name="TParam7"></typeparam>
		/// <returns></returns>
		IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(
			IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> cmd,
			IDbContext cx, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7);

		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning data model results.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="behavior"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <param name="param8"></param>
		/// <typeparam name="TParam"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <typeparam name="TParam2"></typeparam>
		/// <typeparam name="TParam3"></typeparam>
		/// <typeparam name="TParam4"></typeparam>
		/// <typeparam name="TParam5"></typeparam>
		/// <typeparam name="TParam6"></typeparam>
		/// <typeparam name="TParam7"></typeparam>
		/// <typeparam name="TParam8"></typeparam>
		/// <returns></returns>
		IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(
			IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> cmd,
			IDbContext cx, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8);

		/// <summary>
		/// Executes the external command, binding the specified parameters, and returning data model results.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cx"></param>
		/// <param name="behavior"></param>
		/// <param name="param"></param>
		/// <param name="param1"></param>
		/// <param name="param2"></param>
		/// <param name="param3"></param>
		/// <param name="param4"></param>
		/// <param name="param5"></param>
		/// <param name="param6"></param>
		/// <param name="param7"></param>
		/// <param name="param8"></param>
		/// <param name="param9"></param>
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
		/// <returns></returns>
		IDataModelQueryResult<TDataModel> ExecuteMany<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(IDataModelQueryManyCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> cmd,
			IDbContext cx, QueryBehavior behavior, TParam param, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9);
	
	}
}
