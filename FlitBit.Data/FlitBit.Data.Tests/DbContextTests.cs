using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Common;
using System.Data;

namespace FlitBit.Data.Tests
{
	/// <summary>
	/// Summary description for DbContextTests
	/// </summary>
	[TestClass]
	public class DbContextTests
	{
		public TestContext TestContext { get; set; }

		[TestMethod]
		public void EmtpyDbContext()
		{
			using (IDbContext ctx = DbContext.SharedOrNewContext())
			{
				Assert.IsNotNull(ctx, "DbContext should not be null");
			}
		}

		//[TestMethod]
		public void QuerySystemIndexWithinContext()
		{
			using (IDbContext ctx = DbContext.SharedOrNewContext())
			{
				Assert.IsNotNull(ctx, "DbContext should not be null");

				var cn = ctx.NewConnection("windows-search");
				cn.EnsureConnectionIsOpen();

				foreach (DbDataReader reader in cn.ExecuteReader("SELECT TOP 1000 System.ItemPathDisplay FROM SYSTEMINDEX"))
				{
					Assert.IsNotNull(reader.GetString(0));
				}

			}
		}

		[TestMethod]
		public void QuerySystemIndexWithinContext_MoreProperties()
		{
			using (IDbContext ctx = DbContext.SharedOrNewContext())
			{
				Assert.IsNotNull(ctx, "DbContext should not be null");

				var cn = ctx.NewConnection("windows-search");
				cn.EnsureConnectionIsOpen();

				var items = from r in cn.ExecuteReader(@"
SELECT TOP 1000 
	System.ItemName
	, System.ItemNameDisplay
FROM SYSTEMINDEX")
										select new
										{
											ItemName = r.GetString(0),
											ItemNameDisplay = r.GetString(1),
										};
				foreach (var i in items)
				{
					Assert.IsNotNull(i.ItemName);
				}

			}
		}

		//[TestMethod]
		public void QueryDefaultConnectionWithinContext_ExploreSchema()
		{
			using (IDbContext ctx = DbContext.SharedOrNewContext())
			{
				Assert.IsNotNull(ctx, "DbContext should not be null");

				// This code requires MARS to be turned on for the default connection!
				var cn = ctx.NewConnection("default");
				cn.EnsureConnectionIsOpen();
				var schema = from r in cn.ExecuteReader(@"SELECT CATALOG_NAME
	, SCHEMA_NAME
	, SCHEMA_OWNER
	, DEFAULT_CHARACTER_SET_CATALOG
	, DEFAULT_CHARACTER_SET_SCHEMA
	, DEFAULT_CHARACTER_SET_NAME
FROM INFORMATION_SCHEMA.SCHEMATA
ORDER BY CATALOG_NAME, SCHEMA_NAME")
										 select new
										 {
											 Catalog_Name = r.GetValueOrDefault<string>(0),
											 Schema_Name = r.GetValueOrDefault<string>(1),
											 Schema_Owner = r.GetValueOrDefault<string>(2),
											 Default_Character_Set_Catalog = r.GetValueOrDefault<string>(3),
											 Default_Character_Set_Schema = r.GetValueOrDefault<string>(4),
											 Default_Character_Set_Name = r.GetValueOrDefault<string>(5)
										 };
				foreach (var i in schema)
				{
					var cmd = cn.CreateCommand(@"SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = @catalog	AND TABLE_SCHEMA = @schema")
											.BindParameters(binder =>
												{
													binder.DefineAndBindParameter("@catalog", i.Catalog_Name);
													binder.DefineAndBindParameter("@schema", i.Schema_Name);
												});
					var table = from r in cmd.ExecuteEnumerable()
											select new
											{
												Table_Catalog = r.GetValueOrDefault<string>(0),
												Table_Schema = r.GetValueOrDefault<string>(1),
												Table_Name = r.GetValueOrDefault<string>(2),
												Table_Type = r.GetValueOrDefault<string>(3)
											};
					foreach (var t in table)
					{
						Console.Out.WriteLine("{0} [{1}].[{2}].[{3}]", t.Table_Type, t.Table_Catalog, t.Table_Schema, t.Table_Name);
					}
				}
			}
		}
	}
}