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
	public interface IDataModelRepository<TDataModel, TIdentityKey, in TDbConnection> : IDataRepository<TDataModel, TIdentityKey>
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

	}
}
