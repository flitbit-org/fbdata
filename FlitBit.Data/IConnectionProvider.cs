using System;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace FlitBit.Data
{
    /// <summary>
    /// Provides db connection strings.
    /// </summary>
    [ContractClass(typeof(CodeContracts.ContractsForIConnectionProvider))]
    public interface IConnectionProvider
    {
        /// <summary>
        /// Indicates whether the provider can create a connection for the specified name.
        /// </summary>
        /// <param name="name">The connection's name.</param>
        /// <returns><em>true</em> if the provider can create the specified connection; otherwise <em>false</em></returns>
        bool CanCreate(string name);

        /// <summary>
        /// Gets a connection for the specified name.
        /// </summary>
        /// <param name="name">The connection's name.</param>
        /// <returns>A connection for the specified name.</returns>
        /// <exception cref="ArgumentOutOfRangeException">thrown if the provider cannot provide a connection for the specified name.</exception>
        IConnection GetConnection(string name);

        /// <summary>
        /// Gets a connection for the specified name, of type TDbConnection.
        /// </summary>
        /// <param name="name">the connection's name</param>
        /// <typeparam name="TDbConnection">the connection's type</typeparam>
        /// <returns>A connection for the specified name.</returns>
        /// <exception cref="ArgumentOutOfRangeException">thrown if the provider cannot provide a connection for the specified name.</exception>
        /// <exception cref="InvalidCastException">thrown if the connection's type is not a TDbConnection.</exception>
        IConnection<TDbConnection> GetConnection<TDbConnection>(string name)
            where TDbConnection : DbConnection;
    }

    namespace CodeContracts
    {
        /// <summary>
        ///   CodeContracts Class for IConnectionProvider
        /// </summary>
        [ContractClassFor(typeof(IConnectionProvider))]
        internal abstract class ContractsForIConnectionProvider : IConnectionProvider
        {
            public bool CanCreate(string name)
            {
                Contract.Requires<ArgumentNullException>(name != null);
                Contract.Requires<ArgumentException>(name.Length > 0, "Connection name cannot be empty.");

                throw new NotImplementedException();
            }

            public IConnection GetConnection(string name)
            {
                Contract.Requires<ArgumentNullException>(name != null);
                Contract.Requires<ArgumentException>(name.Length > 0, "Connection name cannot be empty.");
                Contract.Ensures(Contract.Result<IConnection>() != null);

                throw new NotImplementedException();
            }

            public IConnection<TDbConnection> GetConnection<TDbConnection>(string name)
                where TDbConnection : DbConnection
            {
                Contract.Requires<ArgumentNullException>(name != null);
                Contract.Requires<ArgumentException>(name.Length > 0, "Connection name cannot be empty.");
                Contract.Ensures(Contract.Result<IConnection<TDbConnection>>() != null);

                throw new NotImplementedException();
            }
        }
    }
}
