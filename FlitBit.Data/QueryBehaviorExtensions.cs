using System;
using System.Diagnostics.Contracts;

namespace FlitBit.Data
{
	/// <summary>
	/// QueryBehavior extensions.
	/// </summary>
	public static class QueryBehaviorExtensions
	{
		/// <summary>
		/// Creates a query behavior object transformed to represent the next page of results.
		/// </summary>
		/// <param name="queryBehavior"></param>
		/// <returns></returns>
		public static QueryBehavior NextPage(this QueryBehavior queryBehavior)
		{
			Contract.Requires<ArgumentNullException>(queryBehavior != null);
			Contract.Requires<InvalidOperationException>(queryBehavior.IsPaging);
			Contract.Requires<InvalidOperationException>(queryBehavior.PageCount < 0 ||
				queryBehavior.Page < queryBehavior.PageCount);
			Contract.Ensures(Contract.Result<QueryBehavior>() != null);
			return new QueryBehavior(queryBehavior.Behaviors, queryBehavior.PageSize, queryBehavior.Page + 1,
															queryBehavior.PageCorrelationKey, false);
		}

		/// <summary>
		/// Creates a query behavior object transformed to represent the next page of results.
		/// </summary>
		/// <param name="queryBehavior"></param>
		/// <returns></returns>
		public static QueryBehavior PriorPage(this QueryBehavior queryBehavior)
		{
			Contract.Requires<ArgumentNullException>(queryBehavior != null);
			Contract.Requires<InvalidOperationException>(queryBehavior.IsPaging);
			Contract.Requires<InvalidOperationException>(queryBehavior.PageCount < 0 || queryBehavior.Page > 0);
			Contract.Ensures(Contract.Result<QueryBehavior>() != null);
			return new QueryBehavior(queryBehavior.Behaviors, queryBehavior.PageSize, queryBehavior.Page - 1,
															queryBehavior.PageCorrelationKey, true);
		}
	}
}