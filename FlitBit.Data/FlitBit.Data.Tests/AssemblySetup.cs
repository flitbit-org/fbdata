using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using FlitBit.Emit;
using FlitBit.Wireup;
using Moq;
using NUnit.Framework;

namespace FlitBit.Data.Tests
{
	[SetUpFixture]
	public class AssemblySetup
	{
	    internal static string FullDbFilePath;

	    [SetUp]
	    public static void AssemblyInit()
	    {
	        RuntimeAssemblies.WriteDynamicAssemblyOnExit = true;
	        WireupCoordinator.SelfConfigure();

	        var uniqueFileName = Guid.NewGuid().ToString("N");

	        // Move the database to the test directory...
	        var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	        Assert.IsNotNull(baseDir);
	        var asm = Assembly.GetExecutingAssembly();
            // Copy the db...
	        using (var dbResource = asm.GetManifestResourceStream("FlitBit.Data.Tests.Databases.AdoWrapperTests.mdf"))
	        {
	            Assert.IsNotNull(dbResource);
	            FullDbFilePath = Path.Combine(baseDir, String.Concat(uniqueFileName, ".mdf"));
	            using (var dbFile = File.Create(FullDbFilePath))
	            {
	                dbResource.CopyTo(dbFile);
	            }
	        }
            // Copy the db...
            using (var logResource = asm.GetManifestResourceStream("FlitBit.Data.Tests.Databases.AdoWrapperTests_log.ldf"))
            {
                Assert.IsNotNull(logResource);
                using (var logFile = File.Create(Path.Combine(baseDir, String.Concat(uniqueFileName, "_log.ldf"))))
                {
                    logResource.CopyTo(logFile);
                }
            }

            // Give the mock provider priority 0 so it is invoked first.
            ConnectionProviders.Instance.Add(MockProvider(), 0);
	    }

        const string FileSourceName = "adoWrapper";

        static IConnectionProvider MockProvider()
        {
            var prov = new Mock<IConnectionProvider>();
            prov.Setup(t => t.CanCreate(FileSourceName)).Returns(true);
            prov.Setup(t => t.GetConnection(FileSourceName))
                     .Returns<string>(n =>
                     {
                         // Adapt the connection string for our DB run.
                         var cn = DbExtensions.CreateConnection(FileSourceName);
                         cn.ConnectionString = cn.ConnectionString.Replace("AdoWrapperTests.mdf", FullDbFilePath);
                         return new DefaultConnection(n, cn);
                     });
            SetupGetConnection<DbConnection>(prov);
            SetupGetConnection<SqlConnection>(prov);

            prov.Setup(t => t.CanCreate(It.IsNotIn(FileSourceName))).Returns(false);
            prov.Setup(t => t.GetConnection(It.IsNotIn(FileSourceName))).Throws<ArgumentOutOfRangeException>();
            return prov.Object;
        }

	    static void SetupGetConnection<T>(Mock<IConnectionProvider> mock)
            where T: DbConnection
	    {
            mock.Setup(t => t.GetConnection<T>(FileSourceName))
                     .Returns<string>(n =>
                     {
                         // Adapt the connection string for our DB run.
                         var cn = (T)DbExtensions.CreateConnection(FileSourceName);
                         cn.ConnectionString = cn.ConnectionString.Replace("AdoWrapperTests.mdf", FullDbFilePath);
                         return new DefaultConnection<T>(n, cn);
                     });
	    }

	    [TearDown]
		public static void AssemblyClean()
		{
			Console.WriteLine(WireupCoordinator.Instance.ReportWireupHistory());
		}
	}
}
