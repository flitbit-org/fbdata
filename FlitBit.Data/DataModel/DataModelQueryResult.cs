#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace FlitBit.Data.DataModel
{
    /// <summary>
    ///     Result object for data model queries.
    /// </summary>
    public class DataModelQueryResult
    {
        /// <summary>
        ///     Creates a new successful result.
        /// </summary>
        /// <param name="behavior">the query's behavior</param>
        protected DataModelQueryResult(QueryBehavior behavior)
        {
            this.Succeeded = true;
            this.Behaviors = behavior;
        }

        /// <summary>
        ///     Creates a new faulted result.
        /// </summary>
        /// <param name="behavior">the query's behavior</param>
        /// <param name="fault">the exception raised by the command</param>
        protected DataModelQueryResult(QueryBehavior behavior, Exception fault)
        {
            this.Succeeded = false;
            this.Behaviors = behavior;
            this.Exception = fault;
        }

        /// <summary>
        ///     Indicates the query's behaviors.
        /// </summary>
        public QueryBehavior Behaviors { get; private set; }

        /// <summary>
        ///     If faulted, indicates the exception raised (as inner exception).
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        ///     Indicates whether the query faulted.
        /// </summary>
        public bool IsFaulted { get { return Exception != null; } }

        /// <summary>
        ///     Indicates whether the query succeeded.
        /// </summary>
        public bool Succeeded { get; private set; }
    }

    /// <summary>
    ///     Interface for strongly-typed data model query results.
    /// </summary>
    /// <typeparam name="TModel">the model type</typeparam>
    public interface IDataModelQueryResult<out TModel>
    {
        /// <summary>
        ///     Indicates the query's behaviors.
        /// </summary>
        QueryBehavior Behaviors { get; }

        /// <summary>
        ///     If faulted, indicates the exception raised (as inner exception).
        /// </summary>
        Exception Exception { get; }

        /// <summary>
        ///     Indicates whether the query faulted.
        /// </summary>
        bool IsFaulted { get; }

        /// <summary>
        ///     Indicates whether the query succeeded.
        /// </summary>
        bool Succeeded { get; }

        /// <summary>
        ///     Gets the results.
        /// </summary>
        /// <exception cref="DataModelException">
        ///     if the underlying data operation thew an exception it will be wrapped in a DataModelException and rethrown
        ///     when accessing the results.
        /// </exception>
        IEnumerable<TModel> Results { get; }
    }

    /// <summary>
    ///     Strongly-typed data model query results.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public sealed class DataModelQueryResult<TModel> : DataModelQueryResult, IDataModelQueryResult<TModel>
    {
        readonly IEnumerable<TModel> _results;

        /// <summary>
        ///     Creates a success result.
        /// </summary>
        /// <param name="behavior"></param>
        /// <param name="results"></param>
        public DataModelQueryResult(QueryBehavior behavior, IEnumerable<TModel> results)
            : base(behavior)
        {
            Contract.Requires<ArgumentNullException>(results != null);

            _results = results;
        }

        /// <summary>
        ///     Creates a faulted result.
        /// </summary>
        /// <param name="behavior"></param>
        /// <param name="fault"></param>
        public DataModelQueryResult(QueryBehavior behavior, Exception fault)
            : base(behavior, fault) { }

        /// <summary>
        ///     Gets the results.
        /// </summary>
        /// <exception cref="DataModelException">
        ///     if the underlying data operation thew an exception it will be wrapped in a DataModelException and rethrown
        ///     when accessing the results.
        /// </exception>
        public IEnumerable<TModel> Results
        {
            get
            {
                if (IsFaulted)
                {
                    throw new DataModelException("Query fault.", Exception);
                }

                return _results;
            }
        }
    }
}