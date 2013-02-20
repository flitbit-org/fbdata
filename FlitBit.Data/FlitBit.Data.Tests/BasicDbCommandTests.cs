using System;
using System.Data;
using System.Linq;
using FlitBit.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Data
{
	[TestClass]
	public class BasicDbCommandTests
	{
		static readonly string CatalogName = "unittest";

		[TestInitialize]
		public void SetupTestCatalog()
		{
			using (var cn = DbExtensions.CreateAndOpenConnection("test"))
			{
				if (!cn.CatalogExists(CatalogName))
				{
					cn.ImmediateExecuteNonQuery(String.Concat("CREATE DATABASE [", CatalogName, "]"));
				}				
			}			
		}

		[TestMethod]
		public void CanCreateAndDropSchemas()
		{
			var test = new
			{
				SchemaName = String.Concat("test_schema_", DateTime.Now.ToString("yyyy_MM_ddTHH_mm_ss_FFFFFFF"))
			};

			DbExecutable.DefineCommandOnConnection("test-data", String.Concat("CREATE SCHEMA [", test.SchemaName, "]"), CommandType.Text)
				.ImmediateExecuteNonQuery();
		
			var selectcmd = DbExecutable.DefineCommandOnConnection("test-data", "SELECT name FROM sys.schemas", CommandType.Text);

			var foundSchema = selectcmd.ImmediateExecuteTransformAll(record => record.GetString(0))
				.Where(n => test.SchemaName.Equals(n))
				.FirstOrDefault();
			Assert.IsNotNull(foundSchema);

			DbExecutable.DefineCommandOnConnection("test-data", String.Concat("DROP SCHEMA [", test.SchemaName, "]"), CommandType.Text)
				.ImmediateExecuteNonQuery();
		}
	}
}
