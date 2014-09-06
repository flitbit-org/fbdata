using System;
using System.Transactions;
using NUnit.Framework;

namespace FlitBit.Data.Tests
{
    [TestFixture]
    public class TransactionScopeHelperTests
    {
        [Test]
        [Explicit("Requires DTC")]
        public void ForcePromotionOfCurrentTransaction_SucceedsWithinTransaction()
        {
            using (new TransactionScope())
            {
                TransactionScopeHelper.ForcePromotionOfCurrentTransaction();
            }
        }

        [Test]
        [Explicit("Requires DTC")]
        [ExpectedException(typeof(InvalidOperationException),
            ExpectedMessage = "Precondition failed: Transaction.Current != null")]
        public void ForcePromotionOfCurrentTransaction_ThrowsWhenNoTransaction()
        {
            TransactionScopeHelper.ForcePromotionOfCurrentTransaction();
        }
    }
}