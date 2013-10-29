using System;
using System.Diagnostics.Contracts;

namespace FlitBit.Data
{
	/// <summary>
	///   Controls the behavior of a repository query.
	/// </summary>
	[Serializable]
	public sealed class QueryBehavior
	{
		/// <summary>
		///   Indicates default query behaviors.
		/// </summary>
		public static QueryBehavior Default = new QueryBehavior(QueryBehaviors.Default);

		private readonly QueryBehaviors _behaviors;
		private readonly int _limit;

		/// <summary>
		///   Constructs a new instance.
		/// </summary>
		/// <param name="behaviors"></param>
		public QueryBehavior(QueryBehaviors behaviors)
		{
			_behaviors = behaviors;
			Page = 1;
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="behaviors"></param>
		/// <param name="limit"></param>
		public QueryBehavior(QueryBehaviors behaviors, int limit)
		{
			Contract.Requires<ArgumentException>(behaviors.HasFlag(QueryBehaviors.Limited));
			Contract.Requires<ArgumentOutOfRangeException>(limit > 0);
			_behaviors = behaviors;
			_limit = limit;
			Page = 1;
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="behaviors"></param>
		/// <param name="pageSize"></param>
		/// <param name="page"></param>
		/// <param name="pageCount"></param>
		public QueryBehavior(QueryBehaviors behaviors, int pageSize, int page, int pageCount)
		{
			Contract.Requires<ArgumentException>(behaviors.HasFlag(QueryBehaviors.Limited));
			Contract.Requires<ArgumentOutOfRangeException>(pageSize > 0);
			Contract.Requires<ArgumentOutOfRangeException>(page > 0, "Page must be 1 based; (Page >= 1).");
			_behaviors = behaviors;
			_limit = pageSize;
			Page = page;
			PageCount = pageCount;
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="behaviors"></param>
		/// <param name="pageSize"></param>
		/// <param name="page"></param>
		/// <param name="correlationKey"></param>
		/// <param name="backward">indicates whether paging backwards</param>
		public QueryBehavior(QueryBehaviors behaviors, int pageSize, int page, object correlationKey, bool backward)
		{
			Contract.Requires<ArgumentException>(behaviors.HasFlag(QueryBehaviors.Limited));
			Contract.Requires<ArgumentOutOfRangeException>(pageSize > 0);
			Contract.Requires<ArgumentOutOfRangeException>(page > 0, "Page must be 1 based; (Page >= 1).");

			_behaviors = behaviors;
			_limit = pageSize;
			Page = page;
			PageCorrelationKey = correlationKey;
			Backward = backward;
		}

		/// <summary>
		///   Indicates the query's behaviors.
		/// </summary>
		public QueryBehaviors Behaviors
		{
			get { return _behaviors; }
		}

		/// <summary>
		///   Indicates that the cache should be bypassed.
		/// </summary>
		public bool BypassCache
		{
			get { return _behaviors.HasFlag(QueryBehaviors.NoCache); }
		}

		/// <summary>
		///   Indicates the query's results should be limited (range or TOP).
		/// </summary>
		public bool IsLimited
		{
			get { return _behaviors.HasFlag(QueryBehaviors.Limited); }
		}

		/// <summary>
		///   Indicates the query's results should be paged.
		/// </summary>
		public bool IsPaging
		{
			get { return _behaviors.HasFlag(QueryBehaviors.Paged); }
		}

		/// <summary>
		///   Indicates the query's limit.
		/// </summary>
		public int Limit
		{
			get { return _limit; }
		}

		/// <summary>
		///   Indicates a query's current page when paging.
		/// </summary>
		public int Page { get; private set; }

		/// <summary>
		///   Used by the framework to correlate paging operations.
		/// </summary>
		public object PageCorrelationKey { get; internal set; }

		/// <summary>
		///   Indicates the total number of pages in the query.
		/// </summary>
		public int PageCount { get; internal set; }

		/// <summary>
		///   Indicates the query's page size.
		/// </summary>
		public int PageSize
		{
			get { return _limit; }
		}

		/// <summary>
		///   Indicates whether the query is paging backwards.
		/// </summary>
		public bool Backward { get; set; }
	}
}