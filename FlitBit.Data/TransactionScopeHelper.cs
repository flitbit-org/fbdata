#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using System.Transactions;

namespace FlitBit.Data
{
	/// <summary>
	/// Static helper class for working with transactions (distributed).
	/// </summary>
	public static class TransactionScopeHelper
	{
		public static readonly bool AutoForceTransactionPromotion = false;
		public static readonly IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadCommitted;
		public static readonly TimeSpan DefaultTransactionTimeout = TimeSpan.FromSeconds(-1);
		internal static readonly string __TransactionPropagationTokenName = "TransactionPropagationToken";

    /// <summary>
    /// Ensures that an ambient transaction is present.
    /// </summary>
    /// <exception cref="InvalidOperationException">thrown if there is not currently an ambient transaction.</exception>
		public static void AssertTransaction()
		{
			if (Transaction.Current == null)
			{
				throw new InvalidOperationException("This operation is only valid within a transaction.");
			}
		}

    /// <summary>
    /// Creates a new transaction using defaults.
    /// </summary>
    /// <returns></returns>
		public static TransactionScope CreateScope_RequireNew()
		{
			return CreateScope_RequireNew(DefaultIsolationLevel, DefaultTransactionTimeout);
		}

    /// <summary>
    /// Creates a new transaction with the specified transaction isolation level.
    /// </summary>
    /// <param name="isolation"></param>
    /// <returns></returns>
		public static TransactionScope CreateScope_RequireNew(IsolationLevel isolation)
		{
			return CreateScope_RequireNew(isolation, DefaultTransactionTimeout);
		}

    /// <summary>
    /// Creates a new transaction with the specified transaction isolation level and timeout.
    /// </summary>
    /// <param name="isolation"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000",
			Justification = "By design; the purpose of the method is to construct a TransactionScope")]
		public static TransactionScope CreateScope_RequireNew(IsolationLevel isolation, TimeSpan timeout)
		{
			var options = new TransactionOptions();
			options.IsolationLevel = isolation;
			if (timeout.Ticks > 0)
			{
				options.Timeout = timeout;
			}
			var result = new TransactionScope(TransactionScopeOption.RequiresNew, options,
																				EnterpriseServicesInteropOption.Automatic);
			if (AutoForceTransactionPromotion)
			{
				ForcePromotionOfCurrentTransaction();
			}
			return result;
		}

    /// <summary>
    /// Shares the current transaction scope, creating one if there isn't an ambient transaction already
    /// in place.
    /// </summary>
    /// <returns></returns>
		public static TransactionScope CreateScope_ShareCurrentOrCreate()
		{
			return CreateScope_ShareCurrentOrCreate(DefaultIsolationLevel, DefaultTransactionTimeout);
		}

    /// <summary>
    /// Shares the current transaction scope, creating one with the specified transaction isolation level
    /// if there isn't an ambient transaction already in place.
    /// </summary>
    /// <param name="isolation"></param>
    /// <returns></returns>
		public static TransactionScope CreateScope_ShareCurrentOrCreate(IsolationLevel isolation)
		{
			return CreateScope_ShareCurrentOrCreate(isolation, DefaultTransactionTimeout);
		}

    /// <summary>
    /// Shares the current transaction scope, creating one with the specified timeout
    /// if there isn't an ambient transaction already in place.
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
		public static TransactionScope CreateScope_ShareCurrentOrCreate(TimeSpan timeout)
		{
			return CreateScope_ShareCurrentOrCreate(DefaultIsolationLevel, timeout);
		}

    /// <summary>
    /// Shares the current transaction scope, creating one with the specified transaction
    /// isolation level and timeout if there isn't an ambient transaction already in place.
    /// </summary>
    /// <param name="isolation"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000",
			Justification = "By design; the purpose of the method is to construct a TransactionScope")]
		public static TransactionScope CreateScope_ShareCurrentOrCreate(IsolationLevel isolation, TimeSpan timeout)
		{
			var existing = Transaction.Current;
			var options = new TransactionOptions();
			options.IsolationLevel = isolation;
			if (timeout.Ticks > 0)
			{
				options.Timeout = timeout;
			}
			var result = new TransactionScope(TransactionScopeOption.Required, options, EnterpriseServicesInteropOption.Automatic);
			// Only promote if it is a new ambient transaction...
			if (existing == null && AutoForceTransactionPromotion)
			{
				ForcePromotionOfCurrentTransaction();
			}
			return result;
		}

    /// <summary>
    /// Creates a transaction scope to suppress the ambient transaction.
    /// </summary>
    /// <returns></returns>
		public static TransactionScope CreateScope_Suppress()
		{
			return new TransactionScope(TransactionScopeOption.Suppress);
		}

		public static TransactionScope CreateScope_SuppressCurrentTransaction()
		{
			return CreateScope_SuppressCurrentTransaction(DefaultIsolationLevel, DefaultTransactionTimeout);
		}

		public static TransactionScope CreateScope_SuppressCurrentTransaction(IsolationLevel isolation)
		{
			return CreateScope_SuppressCurrentTransaction(isolation, DefaultTransactionTimeout);
		}

		public static TransactionScope CreateScope_SuppressCurrentTransaction(TimeSpan timeout)
		{
			return CreateScope_SuppressCurrentTransaction(DefaultIsolationLevel, timeout);
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000",
			Justification = "By design; the purpose of the method is to construct a TransactionScope")]
		public static TransactionScope CreateScope_SuppressCurrentTransaction(IsolationLevel isolation, TimeSpan timeout)
		{
			var options = new TransactionOptions();
			options.IsolationLevel = isolation;
			if (timeout.Ticks > 0)
			{
				options.Timeout = timeout;
			}
			return new TransactionScope(TransactionScopeOption.Suppress, options, EnterpriseServicesInteropOption.Automatic);
		}

    /// <summary>
    /// Forceably promotes the ambient transaction to a distributed transaction. 
    /// </summary>
		public static void ForcePromotionOfCurrentTransaction()
		{
      AssertTransaction();
			TransactionInterop.GetTransmitterPropagationToken(Transaction.Current);
		}

		internal static void CheckTransactionConfidenceFromCallContext(LogicalCallContext lcc)
		{
			if (lcc != null && lcc.HasInfo)
			{
				var trans = Transaction.Current;
				if (trans != null)
				{
					var ctx = (TransactionCallContext) lcc.GetData(__TransactionPropagationTokenName);
					if (ctx != null && ctx.RemoteVoteOfNoConfidence)
					{
						// echo the vote
						trans.Rollback();
					}
				}
			}
		}

		internal static TransactionScope ResurrectTransactionFromCallContext(LogicalCallContext lcc)
		{
			TransactionScope scope = null;
			if (lcc != null && lcc.HasInfo)
			{
				var ctx = (TransactionCallContext) lcc.GetData(__TransactionPropagationTokenName);
				if (ctx != null)
				{
					scope =
						new TransactionScope(TransactionInterop.GetTransactionFromTransmitterPropagationToken(ctx.GetPropagationToken()));
				}
			}
			return scope;
		}

		internal static bool TransmitTransactionWithCallContext(LogicalCallContext lcc, bool voteNoConfidence)
		{
			var result = false;
			var trans = Transaction.Current;
			if (trans != null && lcc != null)
			{
				var tcc = new TransactionCallContext(TransactionInterop.GetTransmitterPropagationToken(trans), voteNoConfidence);
				lcc.SetData(__TransactionPropagationTokenName, tcc);
				result = true;
			}
			return result;
		}
	}

	/// <summary>
	/// Call context wrapper for distributing the ambient transaction.
	/// </summary>
	[Serializable]
	public class TransactionCallContext : ILogicalThreadAffinative
	{
	  readonly byte[] _token;

		internal TransactionCallContext(byte[] token, bool voteNoConfidence)
		{
			this._token = token;
			this.RemoteVoteOfNoConfidence = voteNoConfidence;
		}

		TransactionCallContext()
		{}

		/// <summary>
		/// Indicates the remote end does not have confidence in its end of the
		/// transactioned operations. The local side should not complete the 
		/// transaction.
		/// </summary>
		public bool RemoteVoteOfNoConfidence { get; private set; }

    /// <summary>
    /// Gets the transaction propagation token.
    /// </summary>
    /// <returns></returns>
		public byte[] GetPropagationToken()
		{
			return _token;
		}
	}
}