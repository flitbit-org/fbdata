using System;
using System.Collections.Generic;
using System.Diagnostics;
using FlitBit.Core;
using FlitBit.Core.Parallel;
using FlitBit.Data.Tests.Model;
using FlitBit.Wireup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Data.Tests
{
	[TestClass]
	public class DataRepositoryTests
	{
		static readonly string CatalogName = "unittest";
		static readonly string SchemaName = String.Concat("test_schema_", DateTime.Now.ToString("yyyy_MM_ddTHH_mm_ss_FFFFFFF"));
		static readonly string CreateDatabaseCommand = String.Concat("CREATE DATABASE [", CatalogName, "]");
		static readonly string CreateSchemaCommand = String.Concat("CREATE SCHEMA [", SchemaName, "]");
		static readonly string CreatePeepCommand = String.Concat(@"
CREATE TABLE [", SchemaName, @"].[Peeps]
(
	[ID] INT IDENTITY(0,1) NOT NULL	
		CONSTRAINT PK_Peep PRIMARY KEY,
	[Name] NVARCHAR(50) NOT NULL,
	[Description] NVARCHAR(300) NULL,
	[DateCreated] DATETIME NOT NULL
		CONSTRAINT DF_Peep_DateCreated DEFAULT (GETUTCDATE()),
	[DateUpdated] DATETIME NOT NULL
		CONSTRAINT DF_Peep_DateUpdated DEFAULT (GETUTCDATE()),
		CONSTRAINT CK_Peep_DateUpdated CHECK ([DateUpdated] >= [DateCreated])
)");

		[TestInitialize]
		public void SetupTestCatalog()
		{
			WireupCoordinator.SelfConfigure();

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

		[TestMethod]
		public void Monkey()
		{
			var rand = new Random(Environment.TickCount);
			var gen = new DataGenerator();
			var repo = new PeepsRepository("test-data", SchemaName);

			List<Peep> all = new List<Peep>();

			Stopwatch timer = Stopwatch.StartNew();
			using (var ctx = DbContext.NewContext())
			{
				for (int i = 0; i < 1000; i++)
				{
					var name = gen.GetWords(2);

					var oldPeep = repo.ReadByName(ctx, name);
					if (oldPeep != null)
					{
						Assert.AreEqual(name, oldPeep.Name);

						oldPeep.Description = gen.GetWords(rand.Next(6, 80));

						// ensure we don't overflow the field...
						if (oldPeep.Description.Length > 300)
							oldPeep.Description = oldPeep.Description.Substring(0, 300);

						repo.Update(ctx, oldPeep);
					}
					else
					{
						var it = new Peep();
						it.Name = name;
						it.Description = gen.GetWords(rand.Next(6, 80));

						// ensure we don't overflow the field...
						if (it.Description.Length > 300)
							it.Description = it.Description.Substring(0, 300);

						var newPeep = repo.Create(ctx, it);
						Assert.AreEqual(it.Name, newPeep.Name);
						Assert.AreEqual(it.Description, newPeep.Description);
						Assert.AreNotEqual(it.DateCreated, newPeep.DateCreated);
						Assert.AreNotEqual(it.DateUpdated, newPeep.DateUpdated);
						all.Add(newPeep);
					}
				}
			}
			timer.Start();
			Console.WriteLine(String.Concat(all.Count, " random peeps in ", TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).ToString()));
		}
	}
}
