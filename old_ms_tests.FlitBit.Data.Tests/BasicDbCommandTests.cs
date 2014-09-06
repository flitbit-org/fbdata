using System;
using System.Data;
using System.Data.Common;
using FlitBit.Data;
using FlitBit.Wireup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Data
{
	[TestClass]
	public class BasicDbCommandTests
	{
		static readonly string CatalogName = "unittest";
		static readonly string SchemaName = String.Concat("test_schema_", DateTime.Now.ToString("yyyy_MM_ddTHH_mm_ss_FFFFFFF"));
		static readonly string CreateDatabaseCommand = String.Concat("CREATE DATABASE [", CatalogName, "]");
		static readonly string CreateSchemaCommand = String.Concat("CREATE SCHEMA [", SchemaName, "]");
		static readonly string CreatePeepCommand = String.Concat(@"
CREATE TABLE [", SchemaName, @"].[Peeps]
(
	[ID] INT NOT NULL
		CONSTRAINT PK_Peep PRIMARY KEY,
	[Name] NVARCHAR(50) NOT NULL,
	[Description] NVARCHAR(300) NULL,
	[DateCreated] DATETIME NOT NULL
		CONSTRAINT DF_Peep_DateCreated DEFAULT (GETUTCDATE()),
	[DateUpdated] DATETIME NOT NULL
		CONSTRAINT DF_Peep_DateUpdated DEFAULT (GETUTCDATE()),
		CONSTRAINT CK_Peep_DateUpdated CHECK ([DateUpdated] >= [DateCreated])
)");

		static readonly string SelectPeepByNameCommand = String.Concat(@"
SELECT 
  [", SchemaName, @"].[Peeps].[ID],
  [", SchemaName, @"].[Peeps].[Parent],
  [", SchemaName, @"].[Peeps].[Name],
  [", SchemaName, @"].[Peeps].[Description],
  [", SchemaName, @"].[Peeps].[DateCreated],
  [", SchemaName, @"].[Peeps].[DateUpdated]
FROM [", SchemaName, @"].[Peeps]
WHERE [", SchemaName, @"].[Peeps].[Name] = @Name");

		static readonly string SelectPeepByIDCommand = String.Concat(@"
SELECT 
  [", SchemaName, @"].[Peeps].[ID],
  [", SchemaName, @"].[Peeps].[Parent],
  [", SchemaName, @"].[Peeps].[Name],
  [", SchemaName, @"].[Peeps].[Description],
  [", SchemaName, @"].[Peeps].[DateCreated],
  [", SchemaName, @"].[Peeps].[DateUpdated]
FROM [", SchemaName, @"].[Peeps]
WHERE [", SchemaName, @"].[Peeps].[ID] = @ID");

		[TestMethod]
		public void CanCreateAndDropSchemas()
		{
			var gen = new DataGenerator();
			DbConnection cn;

			Assert.IsNull(DbContext.Current, "There shouldn't be a current DbContext");

			using (var cx = DbContext.NewContext())
			{
				Assert.AreSame(cx, DbContext.Current, "Our context should be current");

				cn = cx.NewConnection("test-data");
				Assert.IsFalse(cn.State.HasFlag(ConnectionState.Open), "the connection shouldn't be open");
				cn.EnsureConnectionIsOpen();
				Assert.IsTrue(cn.State.HasFlag(ConnectionState.Open), "the connection should now be open");
			}
			Assert.IsTrue(cn.State == ConnectionState.Closed);
		}

		[TestInitialize]
		public void SetupTestCatalog()
		{
			using (var cx = DbContext.NewContext())
			{
				using (var cn = cx.NewConnection("test"))
				{
					if (!cn.CatalogExists(CatalogName))
					{
						cn.ImmediateExecuteNonQuery(CreateDatabaseCommand);
					}
				}
				var cndata = cx.NewConnection("test-data").EnsureConnectionIsOpen();
				cndata.ImmediateExecuteNonQuery(CreateSchemaCommand);
				cndata.ImmediateExecuteNonQuery(CreatePeepCommand);
			}
		}
	}
}