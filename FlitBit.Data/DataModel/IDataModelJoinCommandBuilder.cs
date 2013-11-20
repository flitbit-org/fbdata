using System;
using System.Linq.Expressions;

namespace FlitBit.Data.DataModel
{
    /// <summary>
	/// Builds a data model command based on joins to the specified types.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TDbConnection"></typeparam>
	/// <typeparam name="TJoin"></typeparam>
	public interface IDataModelJoinCommandBuilder<TDataModel, TDbConnection, TJoin>
	{
		/// <summary>
		/// Specifies join conditions for the data model. The expression must evaluate like a predicate in order to be translated to SQL.
		/// </summary>
		/// <param name="predicate">a predicate expression defining the joinery between the types</param>
		/// <returns></returns>
    IDataModelJoinCommandBuilder<TDataModel, TDbConnection, TJoin> Join(
			Expression<Func<TDataModel, TJoin, bool>> predicate);

    /// <summary>
    /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    IDataModelQueryCommand<TDataModel, TDbConnection, TJoin, TParam> Where<TParam>(
      Expression<Func<TDataModel, TJoin, TParam, bool>> predicate);
	}
}