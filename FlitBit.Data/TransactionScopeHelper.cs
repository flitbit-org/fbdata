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

		public static TransactionScope CreateScope_RequireNew()
		{
			return CreateScope_RequireNew(DefaultIsolationLevel, DefaultTransactionTimeout);
		}
		public static TransactionScope CreateScope_RequireNew(IsolationLevel isolation)
		{
			return CreateScope_RequireNew(isolation, DefaultTransactionTimeout);
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000", Justification = "By design; the purpose of the method is to construct a TransactionScope")]
		public static TransactionScope CreateScope_RequireNew(IsolationLevel isolation, TimeSpan timeout)
		{
			TransactionOptions options = new TransactionOptions();
			options.IsolationLevel = isolation;
			if (timeout.Ticks > 0)
				options.Timeout = timeout;
			TransactionScope result = new TransactionScope(TransactionScopeOption.RequiresNew, options, EnterpriseServicesInteropOption.Automatic);
			if (AutoForceTransactionPromotion) ForcePromotionOfCurrentTransaction();
			return result;
		}

		public static TransactionScope CreateScope_Suppress()
		{
			return new TransactionScope(TransactionScopeOption.Suppress);
		}

		public static TransactionScope CreateScope_ShareCurrentOrCreate()
		{
			return CreateScope_ShareCurrentOrCreate(DefaultIsolationLevel, DefaultTransactionTimeout);
		}
		public static TransactionScope CreateScope_ShareCurrentOrCreate(IsolationLevel isolation)
		{
			return CreateScope_ShareCurrentOrCreate(isolation, DefaultTransactionTimeout);
		}
		public static TransactionScope CreateScope_ShareCurrentOrCreate(TimeSpan timeout)
		{
			return CreateScope_ShareCurrentOrCreate(DefaultIsolationLevel, timeout);
		}
		[SuppressMessage("Microsoft.Reliability", "CA2000", Justification = "By design; the purpose of the method is to construct a TransactionScope")]
		public static TransactionScope CreateScope_ShareCurrentOrCreate(IsolationLevel isolation, TimeSpan timeout)
		{
			Transaction existing = Transaction.Current;
			TransactionOptions options = new TransactionOptions();
			options.IsolationLevel = isolation;
			if (timeout.Ticks > 0)
				options.Timeout = timeout;
			TransactionScope result = new TransactionScope(TransactionScopeOption.Required, options, EnterpriseServicesInteropOption.Automatic);
			// Only promote if it is a new ambient transaction...
			if (existing == null && AutoForceTransactionPromotion) ForcePromotionOfCurrentTransaction();
			return result;
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
		[SuppressMessage("Microsoft.Reliability", "CA2000", Justification = "By design; the purpose of the method is to construct a TransactionScope")]
		public static TransactionScope CreateScope_SuppressCurrentTransaction(IsolationLevel isolation, TimeSpan timeout)
		{
			TransactionOptions options = new TransactionOptions();
			options.IsolationLevel = isolation;
			if (timeout.Ticks > 0)
				options.Timeout = timeout;
			return new TransactionScope(TransactionScopeOption.Suppress, options, EnterpriseServicesInteropOption.Automatic);
		}

		public static void AssertTransaction()
		{
			if (Transaction.Current == null)
				throw new InvalidOperationException("This operation is only valid within a transaction.");
		}

		public static void ForcePromotionOfCurrentTransaction()
		{
			TransactionInterop.GetTransmitterPropagationToken(Transaction.Current);
		}
		internal static TransactionScope ResurrectTransactionFromCallContext(LogicalCallContext lcc)
		{
			TransactionScope scope = null;
			if (lcc != null && lcc.HasInfo)
			{
				TransactionCallContext ctx = (TransactionCallContext)lcc.GetData(__TransactionPropagationTokenName);
				if (ctx != null)
				{
					scope = new TransactionScope(TransactionInterop.GetTransactionFromTransmitterPropagationToken(ctx.GetPropagationToken()));
				}
			}
			return scope;
		}
		internal static bool TransmitTransactionWithCallContext(LogicalCallContext lcc, bool voteNoConfidence)
		{
			bool result = false;
			Transaction trans = Transaction.Current;
			if (trans != null && lcc != null)
			{
				TransactionCallContext tcc = new TransactionCallContext(TransactionInterop.GetTransmitterPropagationToken(trans), voteNoConfidence);
				lcc.SetData(__TransactionPropagationTokenName, tcc);
				result = true;
			}
			return result;
		}
		internal static void CheckTransactionConfidenceFromCallContext(LogicalCallContext lcc)
		{
			if (lcc != null && lcc.HasInfo)
			{
				Transaction trans = Transaction.Current;
				if (trans != null)
				{
					TransactionCallContext ctx = (TransactionCallContext)lcc.GetData(__TransactionPropagationTokenName);
					if (ctx != null && ctx.RemoteVoteOfNoConfidence)
					{
						// echo the vote
						trans.Rollback();
					}
				}
			}
		}		
	}

	[Serializable]
	public class TransactionCallContext : ILogicalThreadAffinative
	{
		byte[] _token;

		private TransactionCallContext() { }

		internal TransactionCallContext(byte[] token, bool voteNoConfidence)
		{
			this._token = token;
			this.RemoteVoteOfNoConfidence = voteNoConfidence;
		}

		public byte[] GetPropagationToken() { return _token; }
		public bool RemoteVoteOfNoConfidence { get; private set; }
	}
}
