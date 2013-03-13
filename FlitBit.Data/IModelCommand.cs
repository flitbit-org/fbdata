using System.Collections.Generic;
using System.Data.Common;

namespace FlitBit.Data
{
	/// <summary>
	///   Interface for model commands.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	public interface IModelCommand<out TModel, in TKey, in TDbConnection>
		where TDbConnection : DbConnection
	{
		int Execute(IDbContext cx, TDbConnection cn, TKey key);
		IEnumerable<TModel> ExecuteMany(IDbContext cx, TDbConnection cn, QueryBehavior behavior, TKey key);
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn, TKey key);
	}

	/// <summary>
	///   Interface for model commands.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	public interface IModelCommand<out TModel, in TDbConnection>
		where TDbConnection : DbConnection
	{
		int Execute(IDbContext cx, TDbConnection cn);
		IEnumerable<TModel> ExecuteMany(IDbContext cx, TDbConnection cn, QueryBehavior behavior);
		TModel ExecuteSingle(IDbContext cx, TDbConnection cn);
	}
}