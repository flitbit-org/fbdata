using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using FlitBit.Data.CodeContracts;

namespace FlitBit.Data
{
    /// <summary>
    ///     An untyped connection to a db.
    /// </summary>
    [ContractClass(typeof(ContractsForIConnection))]
    public interface IConnection
    {
        /// <summary>
        ///     The connection's name.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Indicates whether the connection can be shared.
        /// </summary>
        bool CanShareConnection { get; }

        /// <summary>
        ///     The untyped connection.
        /// </summary>
        DbConnection UntypedDbConnection { get; }
    }

    /// <summary>
    ///     A strongly typed connection to a db.
    /// </summary>
    /// <typeparam name="TDbConnection">the db connection's type</typeparam>
    public interface IConnection<out TDbConnection> : IConnection
        where TDbConnection : DbConnection
    {
        /// <summary>
        ///     The connection.
        /// </summary>
        TDbConnection DbConnection { get; }
    }

    namespace CodeContracts
    {
        /// <summary>
        ///     CodeContracts Class for IConnection
        /// </summary>
        [ContractClassFor(typeof(IConnection))]
        internal abstract class ContractsForIConnection : IConnection
        {
            public string Name
            {
                get
                {
                    Contract.Ensures(Contract.Result<string>() != null);

                    throw new NotImplementedException();
                }
            }

            public bool CanShareConnection { get { throw new NotImplementedException(); } }

            public DbConnection UntypedDbConnection
            {
                get
                {
                    Contract.Ensures(Contract.Result<DbConnection>() != null);

                    throw new NotImplementedException();
                }
            }

            [ContractInvariantMethod]
            void InvariantContracts()
            {
                Contract.Invariant(Name != null);
                Contract.Invariant(this.UntypedDbConnection != null);
            }
        }
    }
}