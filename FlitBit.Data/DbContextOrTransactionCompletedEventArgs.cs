using System;
using System.Transactions;

namespace FlitBit.Data
{
    /// <summary>
    ///     Event argument provided when a db context, or the transaction in which it participates completes.
    /// </summary>
    public class DbContextOrTransactionCompletedEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="hasTransaction"></param>
        /// <param name="status"></param>
        public DbContextOrTransactionCompletedEventArgs(bool hasTransaction, TransactionStatus status)
        {
            this.HasTransactionStatus = hasTransaction;
            this.TransactionStatus = status;
        }

        /// <summary>
        ///     Indicates whether the db context was participating in a transaction.
        /// </summary>
        public bool HasTransactionStatus { get; set; }

        /// <summary>
        ///     The status of the db context's transaction if it participated in one; otherwise TransactionStatus.None.
        /// </summary>
        public TransactionStatus TransactionStatus { get; private set; }
    }
}