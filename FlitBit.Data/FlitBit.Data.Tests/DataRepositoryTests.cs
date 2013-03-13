using System;
using System.Collections.Concurrent;
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
	[DateCreated] DATETIME2 NOT NULL
		CONSTRAINT DF_Peep_DateCreated DEFAULT (GETUTCDATE()),
	[DateUpdated] DATETIME2 NOT NULL
		CONSTRAINT DF_Peep_DateUpdated DEFAULT (GETUTCDATE()),
		CONSTRAINT CK_Peep_DateUpdated CHECK ([DateUpdated] >= [DateCreated])
)";
		static readonly string DropTableCommandFmt = @"
TRUNCATE TABLE [{0}].[Peeps];
DROP TABLE [{0}].[Peeps]";

		static readonly ConcurrentDictionary<string, Tuple<string, string>> _original =
			new ConcurrentDictionary<string, Tuple<string, string>>();

		string _schemaName = String.Concat("test_schema_", DateTime.Now.ToString("yyyy_MM_ddTHH_mm_ss_FFFFFFF"));

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
		public void DbObjects_CreateReadUpdateAndDelete()
		{
			var rand = new Random(Environment.TickCount);
			var gen = new DataGenerator();
			var cs = ConfigurationManager.ConnectionStrings["test-data"].ConnectionString;
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

			var items = new List<Tuple<string, string>>(_original.Values);

			var all = new List<Peep>();
			var queries = 0;
			var hits = 0;

			var timer = Stopwatch.StartNew();
			using (var cn = new SqlConnection(cs))
			{
				cn.Open();

				for (var i = 0; i < 1000; i++)
				{
					var j = rand.Next(0, items.Count - 1);
					var item = items[j];

					var oldPeep = default(Peep);
					using (var cmd = new SqlCommand(ReadByNameCommand, cn))
					{
						queries++;
						cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 50)).Value = item.Item1;
						using (var reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								hits++;
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
						Assert.AreEqual(item.Item1, oldPeep.Name);

						oldPeep.Description = items[rand.Next(0, items.Count - 1)].Item2;

						// ensure we don't overflow the field...
						if (oldPeep.Description.Length > 300)
						{
							oldPeep.Description = oldPeep.Description.Substring(0, 300);
						}

						using (var cmd = new SqlCommand(UpdateCommand, cn))
						{
							queries++;

							cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int)).Value = oldPeep.ID;
							cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 50)).Value = oldPeep.Name;
							cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 300)).Value = oldPeep.Description;
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
					}
					else
					{
						var it = new Peep();
						it.Name = item.Item1;
						it.Description = item.Item2;

						// ensure we don't overflow the field...
						if (it.Description.Length > 300)
						{
							it.Description = it.Description.Substring(0, 300);
						}

						var newPeep = default(Peep);
						using (var cmd = new SqlCommand(InsertCommand, cn))
						{
							queries++;

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
								}
							}
						}
						Assert.AreEqual(it.Name, newPeep.Name);
						Assert.AreEqual(it.Description, newPeep.Description);
						Assert.AreNotEqual(it.DateCreated, newPeep.DateCreated);
						Assert.AreNotEqual(it.DateUpdated, newPeep.DateUpdated);
						all.Add(newPeep);
					}
				}
				timer.Stop();

				Console.WriteLine(String.Concat("Statsistics", Environment.NewLine,
																				Environment.NewLine, "\t", all.Count, " random peeps in ",
																				TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).ToString(),
																				Environment.NewLine, "\t", timer.ElapsedMilliseconds/all.Count, " milliseconds each.",
																				Environment.NewLine, "\tQueries: ", queries,
																				Environment.NewLine, "\tHits: ", hits
														));
			}

			var itemsUpdated = 0;
			using (var cn = new SqlConnection(cs))
			{
				cn.Open();

				foreach (var peep in all)
				{
					if (peep.DateUpdated > peep.DateCreated)
					{
						itemsUpdated++;
					}
					using (var cmd = new SqlCommand(DeleteCommand, cn))
					{
						cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int)).Value = peep.ID;
						Assert.AreEqual(1, cmd.ExecuteNonQuery());
					}
				}
			}
			Console.WriteLine(String.Concat("\tItems updated: ", itemsUpdated));
		}

		[TestInitialize]
		public void Initialize()
		{
			var ran = new Random(Environment.TickCount);
			var gen = new DataGenerator();

			WireupCoordinator.SelfConfigure();
			while (_original.Count < 1000)
			{
				var name = gen.GetWords(ran.Next(1, 3));
				var desc = gen.GetWords(ran.Next(6, 40));
				_original.TryAdd(name, Tuple.Create(name, desc));
			}

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

		[TestMethod]
		public void TableBackedDataRepository_CreateReadUpdateAndDeleteSync()
		{
			var rand = new Random(Environment.TickCount);
			var gen = new DataGenerator();
			var repo = new PeepsRepository("test-data", _schemaName);
			var items = new List<Tuple<string, string>>(_original.Values);

			var all = new List<Peep>();

			var timer = Stopwatch.StartNew();
			using (var ctx = DbContext.NewContext())
			{
				for (var i = 0; i < 1000; i++)
				{
					var j = rand.Next(0, items.Count - 1);
					var item = items[j];

					var oldPeep = repo.ReadByName(ctx, item.Item1);
					if (oldPeep != null)
					{
						Assert.AreEqual(item.Item1, oldPeep.Name);

						oldPeep.Description = items[rand.Next(0, items.Count - 1)].Item2;

						// ensure we don't overflow the field...
						if (oldPeep.Description.Length > 300)
						{
							oldPeep.Description = oldPeep.Description.Substring(0, 300);
						}

						repo.Update(ctx, oldPeep);
					}
					else
					{
						var it = new Peep();
						it.Name = item.Item1;
						it.Description = item.Item2;

						// ensure we don't overflow the field...
						if (it.Description.Length > 300)
						{
							it.Description = it.Description.Substring(0, 300);
						}

						var newPeep = repo.Create(ctx, it);
						Assert.AreEqual(it.Name, newPeep.Name);
						Assert.AreEqual(it.Description, newPeep.Description);
						Assert.AreNotEqual(it.DateCreated, newPeep.DateCreated);
						Assert.AreNotEqual(it.DateUpdated, newPeep.DateUpdated);
						all.Add(newPeep);
					}
				}
				timer.Stop();
				Console.WriteLine(String.Concat("Statsistics", Environment.NewLine,
																				Environment.NewLine, "\t", all.Count, " random peeps in ",
																				TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).ToString(),
																				Environment.NewLine, "\t", timer.ElapsedMilliseconds/all.Count, " milliseconds each.",
																				Environment.NewLine, "\tQueries: ", ctx.QueryCount,
																				Environment.NewLine, "\tCache Attempts: ", ctx.CacheAttempts,
																				Environment.NewLine, "\tCache Hits: ", ctx.CacheHits,
																				Environment.NewLine, "\tCache Puts: ", ctx.CachePuts,
																				Environment.NewLine, "\tCache Removes: ", ctx.CacheRemoves
														));
			}

			var itemsUpdated = 0;
			using (var ctx = DbContext.NewContext())
			{
				foreach (var peep in repo.All(ctx))
				{
					if (peep.DateUpdated > peep.DateCreated)
					{
						itemsUpdated++;
					}
					Assert.IsTrue(repo.Delete(ctx, peep.ID));
				}
			}
			Console.WriteLine(String.Concat("\tItems updated: ", itemsUpdated));
		}

		[TestMethod]
		public void TableBackedDataRepository_CreateReadUpdateAndDeleteSync_DisableCaching()
		{
			var rand = new Random(Environment.TickCount);
			var gen = new DataGenerator();
			var repo = new PeepsRepository("test-data", _schemaName);
			var items = new List<Tuple<string, string>>(_original.Values);

			var all = new List<Peep>();

			var timer = Stopwatch.StartNew();
			using (var ctx = DbContext.NewContext(DbContextBehaviors.DisableCaching))
			{
				for (var i = 0; i < 1000; i++)
				{
					var j = rand.Next(0, items.Count - 1);
					var item = items[j];

					var oldPeep = repo.ReadByName(ctx, item.Item1);
					if (oldPeep != null)
					{
						Assert.AreEqual(item.Item1, oldPeep.Name);

						oldPeep.Description = items[rand.Next(0, items.Count - 1)].Item2;

						// ensure we don't overflow the field...
						if (oldPeep.Description.Length > 300)
						{
							oldPeep.Description = oldPeep.Description.Substring(0, 300);
						}

						repo.Update(ctx, oldPeep);
					}
					else
					{
						var it = new Peep();
						it.Name = item.Item1;
						it.Description = item.Item2;

						// ensure we don't overflow the field...
						if (it.Description.Length > 300)
						{
							it.Description = it.Description.Substring(0, 300);
						}

						var newPeep = repo.Create(ctx, it);
						Assert.AreEqual(it.Name, newPeep.Name);
						Assert.AreEqual(it.Description, newPeep.Description);
						Assert.AreNotEqual(it.DateCreated, newPeep.DateCreated);
						Assert.AreNotEqual(it.DateUpdated, newPeep.DateUpdated);
						all.Add(newPeep);
					}
				}
				timer.Stop();
				Console.WriteLine(String.Concat("Statsistics", Environment.NewLine,
																				Environment.NewLine, "\t", all.Count, " random peeps in ",
																				TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).ToString(),
																				Environment.NewLine, "\t", timer.ElapsedMilliseconds/all.Count, " milliseconds each.",
																				Environment.NewLine, "\tQueries: ", ctx.QueryCount,
																				Environment.NewLine, "\tCache Attempts: ", ctx.CacheAttempts,
																				Environment.NewLine, "\tCache Hits: ", ctx.CacheHits,
																				Environment.NewLine, "\tCache Puts: ", ctx.CachePuts,
																				Environment.NewLine, "\tCache Removes: ", ctx.CacheRemoves
														));
			}

			var itemsUpdated = 0;
			using (var ctx = DbContext.NewContext())
			{
				foreach (var peep in repo.All(ctx))
				{
					if (peep.DateUpdated > peep.DateCreated)
					{
						itemsUpdated++;
					}
					Assert.IsTrue(repo.Delete(ctx, peep.ID));
				}
			}
			Console.WriteLine(String.Concat("\tItems updated: ", itemsUpdated));
		}
	}
}