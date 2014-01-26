using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
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
        private static readonly string CatalogName = "unittest";
        private static readonly string CreateDatabaseCommandFmt = String.Concat("CREATE DATABASE [", CatalogName, "]");
        private static readonly string CreateSchemaCommandFmt = "CREATE SCHEMA [{0}]";
        private static readonly string DropSchemaCommandFmt = "DROP SCHEMA [{0}]";
        private static readonly string CreatePeepCommandFmt = @"
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
        private static readonly string DropTableCommandFmt = @"
TRUNCATE TABLE [{0}].[Peeps];
DROP TABLE [{0}].[Peeps]";

        private static readonly ConcurrentDictionary<string, Tuple<string, string>> _original =
            new ConcurrentDictionary<string, Tuple<string, string>>();

        private string _schemaName = String.Concat("test_schema_", DateTime.Now.ToString("yyyy_MM_ddTHH_mm_ss_FFFFFFF"));

        [TestCleanup]
        public void Cleanup()
        {
            using (IDbContext cx = DbContext.NewContext())
            {
                DbConnection cndata = cx.NewConnection("test-data").EnsureConnectionIsOpen();
                cndata.ImmediateExecuteNonQuery(String.Format(DropTableCommandFmt, _schemaName));
                cndata.ImmediateExecuteNonQuery(String.Format(DropSchemaCommandFmt, _schemaName));
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

            var items = new List<Tuple<string, string>>(_original.Values);

            var all = new List<Peep>();
            int queries = 0;
            int hits = 0;

            Stopwatch timer = Stopwatch.StartNew();
            using (var cn = new SqlConnection(cs))
            {
                cn.Open();

                for (int i = 0; i < 1000; i++)
                {
                    int j = rand.Next(0, items.Count - 1);
                    Tuple<string, string> item = items[j];

                    Peep oldPeep = default(Peep);
                    using (var cmd = new SqlCommand(ReadByNameCommand, cn))
                    {
                        queries++;
                        cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 50)).Value = item.Item1;
                        using (SqlDataReader reader = cmd.ExecuteReader())
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
                            cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 300)).Value =
                                oldPeep.Description;
                            using (SqlDataReader reader = cmd.ExecuteReader())
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

                        Peep newPeep = default(Peep);
                        using (var cmd = new SqlCommand(InsertCommand, cn))
                        {
                            queries++;

                            cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 50)).Value = it.Name;
                            cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 300)).Value =
                                it.Description;
                            using (SqlDataReader reader = cmd.ExecuteReader())
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

            int itemsUpdated = 0;
            using (var cn = new SqlConnection(cs))
            {
                cn.Open();

                foreach (Peep peep in all)
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

            while (_original.Count < 1000)
            {
                string name = gen.GetWords(ran.Next(1, 3));
                string desc = gen.GetWords(ran.Next(6, 40));
                _original.TryAdd(name, Tuple.Create(name, desc));
            }

            _schemaName = String.Concat("test_schema_", DateTime.Now.ToString("yyyy_MM_ddTHH_mm_ss_FFFFFFF"));
            using (IDbContext cx = DbContext.NewContext())
            {
                using (DbConnection cn = cx.NewConnection("test"))
                {
                    if (!cn.CatalogExists(CatalogName))
                    {
                        cn.ImmediateExecuteNonQuery(String.Format(CreateDatabaseCommandFmt, _schemaName));
                    }
                }
                DbConnection cndata = cx.NewConnection("test-data").EnsureConnectionIsOpen();
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

            Stopwatch timer = Stopwatch.StartNew();
            using (IDbContext ctx = DbContext.NewContext())
            {
                for (int i = 0; i < 1000; i++)
                {
                    int j = rand.Next(0, items.Count - 1);
                    Tuple<string, string> item = items[j];

                    Peep oldPeep = repo.ReadByName(ctx, item.Item1);
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

                        Peep newPeep = repo.Create(ctx, it);
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

            int itemsUpdated = 0;
            using (IDbContext ctx = DbContext.NewContext())
            {
                foreach (Peep peep in repo.All(ctx).Results)
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

            Stopwatch timer = Stopwatch.StartNew();
            using (IDbContext ctx = DbContext.NewContext(DbContextBehaviors.DisableCaching))
            {
                for (int i = 0; i < 1000; i++)
                {
                    int j = rand.Next(0, items.Count - 1);
                    Tuple<string, string> item = items[j];

                    Peep oldPeep = repo.ReadByName(ctx, item.Item1);
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

                        Peep newPeep = repo.Create(ctx, it);
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

            int itemsUpdated = 0;
            using (IDbContext ctx = DbContext.NewContext())
            {
                foreach (Peep peep in repo.All(ctx).Results)
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