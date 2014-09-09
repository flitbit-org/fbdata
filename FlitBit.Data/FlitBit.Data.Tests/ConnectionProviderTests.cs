using System;
using Moq;
using NUnit.Framework;

namespace FlitBit.Data.Tests
{
    [TestFixture]
    public class ConnectionProviderTests
    {
        IConnectionProvider _provider;
        const string KnownName = "windows-search";
        const string UnknownName = "no-connection";

        [SetUp]
        public void Setup()
        {
            var prov = new Mock<IConnectionProvider>();
            prov.Setup(t => t.CanCreate(KnownName)).Returns(true);
            prov.Setup(t => t.GetConnection(KnownName))
                .Returns(new DefaultConnection(KnownName, DbExtensions.CreateConnection(KnownName)));

            prov.Setup(t => t.CanCreate(It.IsNotIn(KnownName))).Returns(false);
            prov.Setup(t => t.GetConnection(It.IsNotIn(KnownName))).Throws<ArgumentOutOfRangeException>();
            _provider = prov.Object;
        }

        [Test]
        public void CanCreate_KnownName_ReturnsTrue() { Assert.IsTrue(_provider.CanCreate(KnownName)); }

        [Test]
        public void CanCreate_UnknownName_ReturnsFalse() { Assert.IsFalse(_provider.CanCreate(UnknownName)); }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetConnection_UnknownName_ThrowsException()
        {
            var cn = _provider.GetConnection(UnknownName);
            Assert.Fail("Shoulda thrown exception.");
        }

        [Test]
        public void GetConnection_KnownName_ReturnsConnection()
        {
            var cn = _provider.GetConnection(KnownName);
            Assert.IsNotNull(cn);
            Assert.AreEqual(KnownName, cn.Name);
        }
    }
}