#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Runtime.Remoting.Messaging;
using System.Transactions;

namespace FlitBit.Data
{
  /// <summary>
  ///   Static helper class for working with transactions (distributed).
  /// </summary>
  public static class TransactionScopeHelper
  {
    internal static readonly string TransactionPropagationTokenName = "TransactionPropagationToken";

    /// <summary>
    ///   Forceably promotes the ambient transaction to a distributed transaction.
    /// </summary>
    public static void ForcePromotionOfCurrentTransaction()
    {
      Contract.Requires<InvalidOperationException>(Transaction.Current != null);
      TransactionInterop.GetTransmitterPropagationToken(Transaction.Current);
    }

    internal static void CheckTransactionConfidenceFromCallContext(LogicalCallContext lcc)
    {
      if (lcc != null
          && lcc.HasInfo)
      {
        var trans = Transaction.Current;
        if (trans != null)
        {
          var ctx = (TransactionCallContext)lcc.GetData(TransactionPropagationTokenName);
          if (ctx != null
              && ctx.RemoteVoteOfNoConfidence)
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
      if (lcc != null
          && lcc.HasInfo)
      {
        var ctx = (TransactionCallContext)lcc.GetData(TransactionPropagationTokenName);
        if (ctx != null)
        {
          scope =
            new TransactionScope(
              TransactionInterop.GetTransactionFromTransmitterPropagationToken(ctx.GetPropagationToken()));
        }
      }
      return scope;
    }

    internal static bool TransmitTransactionWithCallContext(LogicalCallContext lcc, bool voteNoConfidence)
    {
      var result = false;
      var trans = Transaction.Current;
      if (trans != null
          && lcc != null)
      {
        var tcc = new TransactionCallContext(TransactionInterop.GetTransmitterPropagationToken(trans), voteNoConfidence);
        lcc.SetData(TransactionPropagationTokenName, tcc);
        result = true;
      }
      return result;
    }
  }

  /// <summary>
  ///   Call context wrapper for distributing the ambient transaction.
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

    TransactionCallContext() { }

    /// <summary>
    ///   Indicates the remote end does not have confidence in its end of the
    ///   transactioned operations. The local side should not complete the
    ///   transaction.
    /// </summary>
    public bool RemoteVoteOfNoConfidence { get; private set; }

    /// <summary>
    ///   Gets the transaction propagation token.
    /// </summary>
    /// <returns></returns>
    public byte[] GetPropagationToken()
    {
      return _token;
    }
  }
}