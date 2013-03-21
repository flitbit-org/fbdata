using System;
using System.Diagnostics.Contracts;

namespace FlitBit.Data
{
	[Flags]
	public enum QueryBehaviors
	{
		/// <summary>
		///   Indicates the default behavior.
		/// </summary>
		Default = 0,

		/// <summary>
		///   Indicates the query should not consider cached data or cache its results.
		/// </summary>
		NoCache = 1,

		/// <summary>
		///   Indicates the number of results should be limited.
		/// </summary>
		Limited = 1 << 1,

		/// <summary>
		///   Indicates the results should be paged.
		/// </summary>
		Paged = 1 << 2,
	}

	/// <summary>
	///   Controls the behavior of a repository query.
	/// </summary>
	[Serializable]
	public sealed class QueryBehavior
	{
		public static QueryBehavior Default = new QueryBehavior(QueryBehaviors.Default);

		readonly QueryBehaviors _behaviors;

		/// <summary>
		///   Constructs a new instance.
		/// </summary>
		/// <param name="behaviors"></param>
		public QueryBehavior(QueryBehaviors behaviors)
		{
			this._behaviors = behaviors;
		}

		public QueryBehavior(QueryBehaviors behaviors, int limit)
		{
			Contract.Requires<ArgumentException>(behaviors.HasFlag(QueryBehaviors.Limited));
			Contract.Requires<ArgumentOutOfRangeException>(limit > 0);
			this._behaviors = behaviors;
			this.Limit = limit;
		}

		public QueryBehavior(QueryBehaviors behaviors, int pageSize, int page)
		{
			Contract.Requires<ArgumentException>(behaviors.HasFlag(QueryBehaviors.Limited));
			Contract.Requires<ArgumentOutOfRangeException>(pageSize > 0);
			this._behaviors = behaviors;
			this.PageSize = pageSize;
			this.Page = page;
		}

		public QueryBehavior(QueryBehaviors behaviors, int pageSize, int page, object correlationKey)
		{
			Contract.Requires<ArgumentException>(behaviors.HasFlag(QueryBehaviors.Limited));
			Contract.Requires<ArgumentOutOfRangeException>(pageSize > 0);
			this._behaviors = behaviors;
			this.PageSize = pageSize;
			this.Page = page;
			this.PageCorrelationKey = correlationKey;
		}

		public QueryBehaviors Behaviors { get { return _behaviors; } }

		public bool BypassCache { get { return _behaviors.HasFlag(QueryBehaviors.NoCache); } }

		public bool IsLimited { get { return _behaviors.HasFlag(QueryBehaviors.Limited); } }
		public bool IsPaging { get { return _behaviors.HasFlag(QueryBehaviors.Paged); } }

		public int Limit { get; private set; }
		public int Page { get; private set; }

		/// <summary>
		///   Used by the framework to correlate paging operations.
		/// </summary>
		public object PageCorrelationKey { get; internal set; }

		public int PageCount { get; internal set; }
		public int PageSize { get; private set; }
	}

	public static class QueryBehaviorExtensions
	{
		public static QueryBehavior NextPage(this QueryBehavior queryBehavior)
		{
			Contract.Requires<ArgumentNullException>(queryBehavior != null);
			Contract.Requires<InvalidOperationException>(queryBehavior.IsPaging);
			Contract.Requires<InvalidOperationException>(queryBehavior.PageCount < 0 ||
				queryBehavior.Page < queryBehavior.PageCount);
			Contract.Ensures(Contract.Result<QueryBehavior>() != null);
			return new QueryBehavior(queryBehavior.Behaviors, queryBehavior.PageSize, queryBehavior.Page + 1,
															queryBehavior.PageCorrelationKey);
		}

		public static QueryBehavior PriorPage(this QueryBehavior queryBehavior)
		{
			Contract.Requires<ArgumentNullException>(queryBehavior != null);
			Contract.Requires<InvalidOperationException>(queryBehavior.IsPaging);
			Contract.Requires<InvalidOperationException>(queryBehavior.PageCount < 0 || queryBehavior.Page > 0);
			Contract.Ensures(Contract.Result<QueryBehavior>() != null);
			return new QueryBehavior(queryBehavior.Behaviors, queryBehavior.PageSize, queryBehavior.Page - 1,
															queryBehavior.PageCorrelationKey);
		}
	}
}