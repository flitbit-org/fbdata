using System;
using Moq;
using NUnit.Framework;

namespace FlitBit.Data.Tests
{
    [TestFixture]
    public class ConnectionProvidersTests
    {
        const string KnownName = "windows-search";
        const string FileSourceName = "adoWrapper";
        const string MockName = "mock";
        const string UnknownName = "no-connection";

        IConnectionProvider MockProvider()
        {
            var prov = new Mock<IConnectionProvider>();
            prov.Setup(t => t.CanCreate(MockName)).Returns(true);
            prov.Setup(t => t.GetConnection(MockName))
                .Returns<string>(n =>
                {
                    var cn = DbExtensions.CreateConnection(FileSourceName);
                    cn.ConnectionString = cn.ConnectionString.Replace("AdoWrapperTests.mdf",
                        AssemblySetup.FullDbFilePath);
                    return new DefaultConnection(n, cn);
                });

            prov.Setup(t => t.CanCreate(It.IsNotIn(MockName))).Returns(false);
            prov.Setup(t => t.GetConnection(It.IsNotIn(MockName))).Throws<ArgumentOutOfRangeException>();
            return prov.Object;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "Precondition failed: config != null")]
        public void Constructor_WithoutConfig_ThrowsException()
        {
            var prs = new ConnectionProviders(10, null);
            Assert.IsNotNull(prs, "should fail above");
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException),
            ExpectedMessage = "Precondition failed: defaultPriority >= 0")]
        public void Constructor_WithNegPriority_ThrowsException()
        {
            var prs = new ConnectionProviders(-2);
            Assert.IsNotNull(prs, "should fail above");
        }

        [Test]
        public void Constructor_WithDefaultPriority_Succeeds_HasEqualDefaultPriority()
        {
            var defaPriority = 10;
            var provider = new ConnectionProviders(defaPriority);
            Assert.IsNotNull(provider);
            Assert.AreEqual(defaPriority, provider.DefaultPriority);
        }

        [Test]
        public void CanCreate_KnownName_ReturnsTrue()
        {
            var provider = new ConnectionProviders();
            Assert.IsNotNull(provider);
            Assert.IsTrue(provider.CanCreate(KnownName));
        }

        [Test]
        public void CanCreate_UnknownName_ReturnsFalse()
        {
            var provider = new ConnectionProviders();
            Assert.IsNotNull(provider);
            Assert.IsFalse(provider.CanCreate(UnknownName));
        }

        [Test]
        public void GetConnection_KnownName_ReturnsConnection()
        {
            var provider = new ConnectionProviders();
            Assert.IsNotNull(provider);

            var cn = provider.GetConnection(KnownName);
            Assert.AreEqual(KnownName, cn.Name);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetConnection_UnknownName_ThrowsException()
        {
            var provider = new ConnectionProviders();
            Assert.IsNotNull(provider);

            provider.GetConnection(UnknownName);
            Assert.Fail("Shoulda thrown exception.");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException), ExpectedMessage = "Precondition failed: provider != null")]
        public void Add_Null_ThrowsException()
        {
            var provider = new ConnectionProviders();
            Assert.IsNotNull(provider);

            provider.Add(null);
        }

        [Test]
        public void Add_MakesNewConnectionsAvailable()
        {
            var provider = new ConnectionProviders();
            Assert.IsNotNull(provider);

            Assert.IsFalse(provider.CanCreate(MockName), "Cannot create before mock provider added.");

            var mock = MockProvider();
            provider.Add(mock);

            Assert.IsTrue(provider.CanCreate(MockName), "Can create after mock provider added.");

            var cn = provider.GetConnection(MockName);
            Assert.IsNotNull(cn);

            using (var dbConn = cn.UntypedDbConnection)
            {
                dbConn.Open();
            }
        }

        [Test]
        public void Instance_CreatesAndReturnsSingleton()
        {
            var one = ConnectionProviders.Instance;
            var two = ConnectionProviders.Instance;
            var three = ConnectionProviders.Instance;
            Assert.AreSame(one, two);
            Assert.AreSame(two, three);
        }
    }
}