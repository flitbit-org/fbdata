using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using FlitBit.Core;
using FlitBit.Data.Tests.Model;
using FlitBit.Wireup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Data.Tests
{
	[TestClass]
	public class DataRepositoryTests
	{
		static readonly string CatalogName = "unittest";
		static readonly string CreateDatabaseCommandFmt = String.Concat("CREATE DATABASE [", CatalogName, "]");
		static readonly string CreateSchemaCommandFmt = "CREATE SCHEMA [{0}]";
		static readonly string DropSchemaCommandFmt = "DROP SCHEMA [{0}]";
		static readonly string CreatePeepCommandFmt = @"
CREATE TABLE [{0}].[Peeps]
(
	[ID] INT IDENTITY(0,1) NOT NULL	
		CONSTRAINT PK_Peep PRIMARY KEY,
	[Name] NVarChar(50) NOT NULL,
	[Description] NVarChar(300) NULL,
	[DateCreated] DATETIME NOT NULL
		CONSTRAINT DF_Peep_DateCreated DEFAULT (GETUTCDATE()),
	[DateUpdated] DATETIME NOT NULL
		CONSTRAINT DF_Peep_DateUpdated DEFAULT (GETUTCDATE()),
		CONSTRAINT CK_Peep_DateUpdated CHECK ([DateUpdated] >= [DateCreated])
)";																																		 
		static readonly string DropTableCommandFmt = @"
TRUNCATE TABLE [{0}].[Peeps];
DROP TABLE [{0}].[Peeps]";

		string _schemaName = String.Concat("test_schema_", DateTime.Now.ToString("yyyy_MM_ddTHH_mm_ss_FFFFFFF"));

		[TestInitialize]
		public void Initialize()
		{
			WireupCoordinator.SelfConfigure();

			_schemaName = String.Concat("test_schema_", DateTime.Now.ToString("yyyy_MM_ddTHH_mm_ss_FFFFFFF"));
			using (var cx = DbContext.NewContext())
			{
				using (var cn = cx.NewConnection("test"))
				{
					if (!cn.CatalogExists(CatalogName))
					{
						cn.ImmediateExecuteNonQuery(String.Format(CreateDatabaseCommandFmt, _schemaName));
					}
				}
				var cndata = cx.NewConnection("test-data").EnsureConnectionIsOpen();
				cndata.ImmediateExecuteNonQuery(String.Format(CreateSchemaCommandFmt, _schemaName));
				cndata.ImmediateExecuteNonQuery(String.Format(CreatePeepCommandFmt, _schemaName));
			}
		}

		[TestCleanup]
		public void Cleanup()
		{																																											
			using (var cx = DbContext.NewContext())
			{
				var cndata = cx.NewConnection("test-data").EnsureConnectionIsOpen();
				cndata.ImmediateExecuteNonQuery(String.Format(DropTableCommandFmt, _schemaName));
				cndata.ImmediateExecuteNonQuery(String.Format(DropSchemaCommandFmt, _schemaName));
			}
		}

		[TestMethod]
		public void TableBackedDataRepository_CreateReadUpdateAndDeleteSync()
		{
			var rand = new Random(Environment.TickCount);
			var gen = new DataGenerator();
			var repo = new PeepsRepository("test-data", _schemaName);

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
				timer.Stop();
			}
			
			Console.WriteLine(String.Concat(all.Count, " random peeps in ", TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).ToString()));
			Console.WriteLine(String.Concat(timer.ElapsedMilliseconds / all.Count, " milliseconds each."));


			using (var ctx = DbContext.NewContext())
			{
				foreach (var peep in all)
				{
					Assert.IsTrue(repo.Delete(ctx, peep.ID));
				}
			}
		}

		[TestMethod]
		public void DbObjects_CreateReadUpdateAndDelete()
		{
			var rand = new Random(Environment.TickCount);
			var gen = new DataGenerator();			 			
			string cs = ConfigurationManager.ConnectionStrings["test-data"].ConnectionString;
			string InsertCommand = String.Format(PeepsRepository.__InsertCommandFmt, _schemaName),
			UpdateCommand = String.Format(PeepsRepository.__UpdateCommandFmt, _schemaName),
			ReadCommand = String.Concat(
				String.Format(PeepsRepository.__BaseSelectCommandFmt, _schemaName),
				String.Format(PeepsRepository.__ByIDCommandFmt, _schemaName)
				),
			ReadByNameCommand = String.Concat(
				String.Format(PeepsRepository.__BaseSelectCommandFmt, _schemaName),
				String.Format(PeepsRepository.__ByNameCommandFmt, _schemaName)
				),
			DeleteCommand = String.Format(PeepsRepository.__DeleteCommandFmt, _schemaName);

			List<Peep> all = new List<Peep>();

			Stopwatch timer = Stopwatch.StartNew();
			using (var cn = new SqlConnection(cs))
			{
				cn.Open();
				
				for (int i = 0; i < 1000; i++)
				{
					var name = gen.GetWords(2);
					Peep oldPeep = default(Peep);
					using (var cmd = new SqlCommand(ReadByNameCommand, cn))
					{
						cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 50)).Value = name;
						using (var reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{	 
								oldPeep = new Peep();
								oldPeep.ID = reader.GetInt32(0);
								oldPeep.Name = reader.GetString(1);
								oldPeep.Description = reader.GetString(2);
								oldPeep.DateCreated = reader.GetDateTime(3);
								oldPeep.DateUpdated = reader.GetDateTime(4); 
							}
						}								
					}
					if (oldPeep != null)
					{
						Assert.AreEqual(name, oldPeep.Name);

						oldPeep.Description = gen.GetWords(rand.Next(6, 80));

						// ensure we don't overflow the field...
						if (oldPeep.Description.Length > 300)
							oldPeep.Description = oldPeep.Description.Substring(0, 300);

						using (var cmd = new SqlCommand(UpdateCommand, cn))
						{
							cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int)).Value = oldPeep.ID;
							cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 50)).Value = oldPeep.Name;
							cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 300)).Value = oldPeep.Description;
							using (var reader = cmd.ExecuteReader())
							{
								if (reader.Read())
								{
									if (reader.Read())
									{
										oldPeep = new Peep();
										oldPeep.ID = reader.GetInt32(0);
										oldPeep.Name = reader.GetString(1);
										oldPeep.Description = reader.GetString(2);
										oldPeep.DateCreated = reader.GetDateTime(3);
										oldPeep.DateUpdated = reader.GetDateTime(4);
									}
								}
							}
						}
					}
					else
					{
						var it = new Peep();
						it.Name = name;
						it.Description = gen.GetWords(rand.Next(6, 80));

						// ensure we don't overflow the field...
						if (it.Description.Length > 300)
							it.Description = it.Description.Substring(0, 300);

						Peep newPeep = default(Peep);
						using (var cmd = new SqlCommand(InsertCommand, cn))
						{
							cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 50)).Value = it.Name;
							cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 300)).Value = it.Description;
							using (var reader = cmd.ExecuteReader())
							{
								if (reader.Read())
								{
										newPeep = new Peep();
										newPeep.ID = reader.GetInt32(0);
										newPeep.Name = reader.GetString(1);
										newPeep.Description = reader.GetString(2);
										newPeep.DateCreated = reader.GetDateTime(3);
										newPeep.DateUpdated = reader.GetDateTime(4);
						}	}
						} 
						Assert.AreEqual(it.Name, newPeep.Name);
						Assert.AreEqual(it.Description, newPeep.Description);
						Assert.AreNotEqual(it.DateCreated, newPeep.DateCreated);
						Assert.AreNotEqual(it.DateUpdated, newPeep.DateUpdated);
						all.Add(newPeep);
					}
				}
				timer.Stop();
			}
			
			Console.WriteLine(String.Concat(all.Count, " random peeps in ", TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).ToString()));
			Console.WriteLine(String.Concat(timer.ElapsedMilliseconds / all.Count, " milliseconds each."));

			using (var cn = new SqlConnection(cs))
			{
				cn.Open();

				foreach (var peep in all)
				{
					using (var cmd = new SqlCommand(DeleteCommand, cn))
					{
						cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int)).Value = peep.ID;
						Assert.AreEqual(1, cmd.ExecuteNonQuery());
					}
				}
			}
		}
	}
}
