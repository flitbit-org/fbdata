#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using System.Transactions;

namespace FlitBit.Data
{
	public static class TransactionScopeHelper
	{
		internal static readonly string __TransactionPropagationTokenName = "TransactionPropagationToken";

		public static readonly IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadCommitted;
		public static readonly TimeSpan DefaultTransactionTimeout = TimeSpan.FromSeconds(-1);
		public static readonly bool AutoForceTransactionPromotion = false;

		public static void AssertTransaction()
		{
			if (Transaction.Current == null)
			{
				throw new InvalidOperationException("This operation is only valid within a transaction.");
			}
		}

		public static TransactionScope CreateScope_RequireNew() { return CreateScope_RequireNew(DefaultIsolationLevel, DefaultTransactionTimeout); }
		public static TransactionScope CreateScope_RequireNew(IsolationLevel isolation) { return CreateScope_RequireNew(isolation, DefaultTransactionTimeout); }

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

		public static TransactionScope CreateScope_ShareCurrentOrCreate() { return CreateScope_ShareCurrentOrCreate(DefaultIsolationLevel, DefaultTransactionTimeout); }
		public static TransactionScope CreateScope_ShareCurrentOrCreate(IsolationLevel isolation) { return CreateScope_ShareCurrentOrCreate(isolation, DefaultTransactionTimeout); }
		public static TransactionScope CreateScope_ShareCurrentOrCreate(TimeSpan timeout) { return CreateScope_ShareCurrentOrCreate(DefaultIsolationLevel, timeout); }

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

		public static TransactionScope CreateScope_Suppress() { return new TransactionScope(TransactionScopeOption.Suppress); }

		public static TransactionScope CreateScope_SuppressCurrentTransaction() { return CreateScope_SuppressCurrentTransaction(DefaultIsolationLevel, DefaultTransactionTimeout); }
		public static TransactionScope CreateScope_SuppressCurrentTransaction(IsolationLevel isolation) { return CreateScope_SuppressCurrentTransaction(isolation, DefaultTransactionTimeout); }
		public static TransactionScope CreateScope_SuppressCurrentTransaction(TimeSpan timeout) { return CreateScope_SuppressCurrentTransaction(DefaultIsolationLevel, timeout); }

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

		public static void ForcePromotionOfCurrentTransaction() { TransactionInterop.GetTransmitterPropagationToken(Transaction.Current); }

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

	[Serializable]
	public class TransactionCallContext : ILogicalThreadAffinative
	{
		byte[] _token;

		TransactionCallContext() { }

		internal TransactionCallContext(byte[] token, bool voteNoConfidence)
		{
			this._token = token;
			this.RemoteVoteOfNoConfidence = voteNoConfidence;
		}

		public bool RemoteVoteOfNoConfidence { get; private set; }
		public byte[] GetPropagationToken() { return _token; }
	}
}