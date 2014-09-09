using System;
using System.Data.Odbc;
using NUnit.Framework;

namespace FlitBit.Data.Tests
{
    [TestFixture]
    public class DefaultConnectionTests
    {
        [Test]
        public void DefaultConnection_CanConstructWhenNameAndConnectionArgumentsArePresent()
        {
            const string name = "bogus";
            var dbCn = new OdbcConnection();

            var cn = new DefaultConnection(name, dbCn);

            Assert.IsNotNull(cn);
            Assert.AreEqual(name, cn.Name);
            Assert.AreEqual(dbCn, cn.UntypedDbConnection);
            Assert.IsFalse(cn.CanShareConnection);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "Precondition failed: name != null")]
        public void DefaultConnection_ConstructorFailsWhenNameMissing()
        {
            var dbCn = new OdbcConnection();

            var cn = new DefaultConnection(null, dbCn);
            Assert.IsNull(cn, "Shouldn't get here.");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "Precondition failed: cn != null")]
        public void DefaultConnection_ConstructorFailsWhenConnectionMissing()
        {
            var cn = new DefaultConnection("oops", null);
            Assert.IsNull(cn, "Shouldn't get here.");
        }

        [Test]
        public void DefaultConnection_ComparesEqualToItself()
        {
            var cn = new DefaultConnection("IsEqualToItself", new OdbcConnection());

            Assert.IsTrue(cn.Equals(cn));
        }

        [Test]
        public void DefaultConnection_HashCodeIsCalculatedUnequalForUnequalInstances()
        {
            var cn = new DefaultConnection("IsEqualToSame", new OdbcConnection());
            var cn2 = new DefaultConnection("IsEqualToSame", new OdbcConnection());

            Assert.IsFalse(cn.Equals(cn2));
            Assert.AreNotEqual(cn.GetHashCode(), cn2.GetHashCode());
        }

        [Test]
        public void DefaultConnection_HashCodeIsCalculatedConsistentlyForEqualInstances()
        {
            var dbCn = new OdbcConnection();

            var cn = new DefaultConnection("IsEqualToSame", dbCn);
            var cn2 = new DefaultConnection("IsEqualToSame", dbCn);

            Assert.IsTrue(cn.Equals(cn2));
            Assert.AreEqual(cn.GetHashCode(), cn2.GetHashCode());
        }
    }
}