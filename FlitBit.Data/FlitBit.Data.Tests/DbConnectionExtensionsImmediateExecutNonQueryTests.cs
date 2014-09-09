using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using FlitBit.Core;
using NUnit.Framework;

namespace FlitBit.Data.Tests
{
    [TestFixture]
    public class DbConnectionExtensionsImmediateExecutNonQueryTests
    {
        readonly string _schemaName = String.Concat("test_schema_", DateTime.Now.ToString("yyyy_MM_ddTHH_mm_ss_FFFFFFF"));

        [SetUp]
        public void Setup()
        {
            using (var cn = ConnectionProviders.GetDbConnection("adoWrapper"))
            {
                cn.Open();
                cn.ImmediateExecuteNonQuery(String.Format("CREATE SCHEMA [{0}]", _schemaName));

                cn.ImmediateExecuteNonQuery(String.Format(@"
CREATE TABLE [{0}].[TestWidgets]
	(
        [ID] INT IDENTITY(1, 1) NOT NULL
			CONSTRAINT PK_TestWidgets PRIMARY KEY,
		[Name] NVARCHAR(24) NOT NULL,
		[Description] NVARCHAR(2000) NULL,
		[Active] BIT NOT NULL
	)
", this._schemaName));
            }
        }

        [TearDown]
        public void Teardown()
        {
            using (var cn = ConnectionProviders.GetDbConnection("adoWrapper"))
            {
                cn.Open();

                cn.ImmediateExecuteNonQuery(String.Format(@"DROP TABLE [{0}].[TestWidgets]", this._schemaName));

                cn.ImmediateExecuteNonQuery(String.Format(@"DROP SCHEMA [{0}]", this._schemaName));
            }
        }

        [Test]
        public void ImmediateExecuteNonQuery_WithBinderAndBoundParameter_Succeeds()
        {
            var itemCount = 1000;
            var stopwatch = new Stopwatch();
            var rand = new Random(Environment.TickCount);
            var gen = new DataGenerator();
            var originalData = new List<Tuple<int, string, string, bool>>();

            using (var cn = ConnectionProviders.GetDbConnection("adoWrapper"))
            {
                cn.Open();

                var items = new List<Tuple<string, string, bool>>();
                for (var i = 0; i < itemCount; ++i)
                {
                    items.Add(Tuple.Create(
                        gen.GetString(rand.Next(12, 24)),
                        gen.GetStringWithLineBreaks(2000),
                        rand.Next() % 3 == 0));
                }

                stopwatch.Start();

                foreach (var item in items)
                {
                    var it = item;
                    Assert.AreEqual(1, cn.ImmediateExecuteNonQuery(
                        String.Format(@"
INSERT INTO [{0}].[TestWidgets] (
  [Name], 
  [Description], 
  [Active] )
VALUES (
  @Name,
  @Description,
  @Active
)
SELECT @ID = SCOPE_IDENTITY()", this._schemaName),
                        binder =>
                        {
                            binder.DefineParameter("ID", DbType.Int32, ParameterDirection.Output);
                            binder.DefineAndBindParameter("Name", it.Item1);
                            binder.DefineAndBindParameter("Description", it.Item2);
                            binder.DefineAndBindParameter("Active", it.Item3);
                        },
                        (cmd, binder, res) => originalData.Add(Tuple.Create(
                            binder.GetInt32(cmd, "ID"),
                            it.Item1,
                            it.Item2,
                            it.Item3)
                                                  )));
                }
            }
            Console.WriteLine("Total time for {0} inserts: {1}", itemCount,
                TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds));
        }

        [Test]
        public void ImmediateExecuteNonQueryBatch_WithBinderAndBoundParameters_Succeeds()
        {
            var itemCount = 1000;
            var stopwatch = new Stopwatch();
            var rand = new Random(Environment.TickCount);
            var gen = new DataGenerator();
            var originalData = new List<Tuple<int, string, string, bool>>();

            using (var cn = ConnectionProviders.GetDbConnection("adoWrapper"))
            {
                cn.Open();

                var items = new List<Tuple<string, string, bool>>();
                for (var i = 0; i < itemCount; ++i)
                {
                    items.Add(Tuple.Create(
                        gen.GetString(rand.Next(12, 24)),
                        gen.GetStringWithLineBreaks(2000),
                        rand.Next() % 3 == 0));
                }

                Console.WriteLine("Time to generate data: {0}", TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds));

                stopwatch.Start();

                var res = cn.ImmediateExecuteNonQueryBatch(
                    items,
                    (cmd, binder) =>
                    {
                        cmd.CommandText = String.Format(@"
INSERT INTO [{0}].[TestWidgets] (
  [Name], 
  [Description], 
  [Active] )
VALUES (
  @Name,
  @Description,
  @Active
)
SELECT @ID = SCOPE_IDENTITY()", this._schemaName);
                        binder.DefineParameter("ID", DbType.Int32, ParameterDirection.Output);
                        binder.DefineParameter("Name", DbType.String, 24);
                        binder.DefineParameter("Description", DbType.String, 2000);
                        binder.DefineParameter("Active", DbType.Boolean);
                    },
                    (cmd, binder, item) =>
                    {
                        binder.SetParameterValue("Name", item.Item1);
                        binder.SetParameterValue("Description", item.Item2);
                        binder.SetParameterValue("Active", item.Item3);
                    },
                    (cmd, binder, item, affected) =>
                    {
                        Assert.AreEqual(1, affected);
                        originalData.Add(Tuple.Create(
                            binder.GetInt32(cmd, "ID"),
                            item.Item1,
                            item.Item2,
                            item.Item3));
                    });
            }

            Console.WriteLine("Total time for {0} inserts: {1}", itemCount,
                TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds));

            // Meh; not really a difference when reusing a prepared command with 1000 items.
        }
    }
}