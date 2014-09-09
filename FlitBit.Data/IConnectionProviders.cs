using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using FlitBit.Data.CodeContracts;

namespace FlitBit.Data
{
    /// <summary>
    ///     Registry of connection providers.
    /// </summary>
    [ContractClass(typeof(ContractsForIConnectionProviders))]
    public interface IConnectionProviders : IConnectionProvider
    {
        /// <summary>
        ///     Gets the default priority assigned to providers.
        /// </summary>
        int DefaultPriority { get; }

        /// <summary>
        ///     Adds a provider with the default priority.
        /// </summary>
        /// <param name="provider">the priority</param>
        void Add(IConnectionProvider provider);

        /// <summary>
        ///     Adds a provider with the specified priority.
        /// </summary>
        /// <param name="provider">the provider</param>
        /// <param name="priority">the priority; must be greater than 0 (zero).</param>
        void Add(IConnectionProvider provider, int priority);

        /// <summary>
        ///     Removes a connection provider.
        /// </summary>
        /// <param name="provider">the provider.</param>
        void Remove(IConnectionProvider provider);
    }

    namespace CodeContracts
    {
        /// <summary>
        ///     CodeContracts Class for IConnectionProvider
        /// </summary>
        [ContractClassFor(typeof(IConnectionProviders))]
        internal abstract class ContractsForIConnectionProviders : IConnectionProviders
        {
            public bool CanCreate(string name)
            {
                Contract.Requires<ArgumentNullException>(name != null);

                throw new NotImplementedException();
            }

            public IConnection GetConnection(string name)
            {
                Contract.Requires<ArgumentNullException>(name != null);
                Contract.Ensures(Contract.Result<IConnection>() != null);

                throw new NotImplementedException();
            }

            public IConnection<TDbConnection> GetConnection<TDbConnection>(string name)
                where TDbConnection : DbConnection
            {
                Contract.Requires<ArgumentNullException>(name != null);
                Contract.Ensures(Contract.Result<IConnection<TDbConnection>>() != null);

                throw new NotImplementedException();
            }

            public int DefaultPriority { get { throw new NotImplementedException(); } }

            public void Add(IConnectionProvider provider)
            {
                Contract.Requires<ArgumentNullException>(provider != null);

                throw new NotImplementedException();
            }

            public void Add(IConnectionProvider provider, int priority)
            {
                Contract.Requires<ArgumentNullException>(provider != null);
                Contract.Requires<ArgumentOutOfRangeException>(priority >= 0);

                throw new NotImplementedException();
            }

            public void Remove(IConnectionProvider provider)
            {
                Contract.Requires<ArgumentNullException>(provider != null);

                throw new NotImplementedException();
            }
        }
    }
}