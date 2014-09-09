using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FlitBit.Core;
using NUnit.Framework;

namespace FlitBit.Data.Tests
{
    [TestFixture]
    public class DbConnectionExtensionsTests
    {
        // These tests use the DbContext, tested elsewhere, in order to create connections.

        [Test]
        public void EnsureConnectionIsOpen_OpensConnection()
        {
            using (var context = DbContext.NewContext())
            {
                var cn = context.SharedOrNewConnection<SqlConnection>("adoWrapper");
                Assert.IsNotNull(cn);

                Assert.AreEqual(ConnectionState.Closed, cn.State);

                cn.EnsureConnectionIsOpen();

                Assert.AreEqual(ConnectionState.Open, cn.State);
            }
        }

        [Test]
        public void CreateCommand_WithCommandText_CreatesCommand()
        {
            using (var context = DbContext.NewContext())
            {
                var cn = context.SharedOrNewConnection<SqlConnection>("adoWrapper");
                Assert.IsNotNull(cn);

                cn.EnsureConnectionIsOpen();

                using (var cmd = cn.CreateCommand("SELECT @@SERVERNAME"))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        Assert.IsTrue(reader.Read());
                        var serverName = reader.GetString(0);
                        Assert.IsNotNull(serverName);
                    }
                }
            }
        }

        [Test]
        public void CreateCommand_WithCommandTextAndCommandType_CreatesCommand()
        {
            var spec =
                new
                {
                    CmdText = "SELECT Top 25 System.ItemPathDisplay FROM SYSTEMINDEX",
                    CmdType = CommandType.Text
                };

            using (var ctx = DbContext.SharedOrNewContext())
            {
                using (var cn = ctx.NewConnection("windows-search"))
                {
                    cn.EnsureConnectionIsOpen();
                    using (var cmd = cn.CreateCommand(spec.CmdText, spec.CmdType))
                    {
                        Assert.AreEqual(spec.CmdText, cmd.CommandText);
                        Assert.AreEqual(spec.CmdType, cmd.CommandType);

                        using (var reader = cmd.ExecuteReader())
                        {
                            Assert.IsTrue(reader.Read());
                        }
                    }
                }
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException),
            ExpectedMessage = "Precondition failed: connection.State.HasFlag(ConnectionState.Open)")]
        public void ImmediateExecuteEnumerable_ThrowsWhenConnectionNotOpen()
        {
            var spec =
                new
                {
                    CmdText = "SELECT Top 25 System.ItemPathDisplay FROM SYSTEMINDEX",
                    CmdType = CommandType.Text
                };

            using (var ctx = DbContext.SharedOrNewContext())
            {
                using (var cn = ctx.NewConnection("windows-search"))
                {
                    foreach (var record in cn.ImmediateExecuteEnumerable(spec.CmdText))
                    {
                        Assert.IsNotNull(record.GetString(0));
                    }
                }
            }
        }

        [Test]
        public void ImmediateExecuteEnumerable_ExecutesAndReturnsEnumerable()
        {
            var spec =
                new
                {
                    CmdText = "SELECT Top 25 System.ItemPathDisplay FROM SYSTEMINDEX",
                    CmdType = CommandType.Text
                };

            using (var ctx = DbContext.SharedOrNewContext())
            {
                using (var cn = ctx.NewConnection("windows-search"))
                {
                    cn.EnsureConnectionIsOpen();

                    Assert.IsTrue(cn.ImmediateExecuteEnumerable(spec.CmdText)
                                    .Select(r => r.GetString(0))
                                    .Any());
                }
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Assertion failed: !String.IsNullOrEmpty(cmd.CommandText)")]
        public void ImmediateExecuteEnumerable_ThrowsWhenPrepareActionFailsToSetCommandText()
        {
            using (var ctx = DbContext.SharedOrNewContext())
            {
                using (var cn = ctx.NewConnection("windows-search"))
                {
                    cn.EnsureConnectionIsOpen();

                    Assert.IsTrue(cn.ImmediateExecuteEnumerable(c =>
                    { /* does nothing */
                    })
                                    .Select(r => r.GetString(0))
                                    .Any());
                }
            }
        }

        [Test]
        public void ImmediateExecuteEnumerable_PreparesAndExecutesAndReturnsEnumerable()
        {
            var spec =
                new
                {
                    CmdText = "SELECT Top 25 System.ItemPathDisplay FROM SYSTEMINDEX",
                    CmdType = CommandType.Text
                };
            var prepareInvoked = false;

            using (var ctx = DbContext.SharedOrNewContext())
            {
                using (var cn = ctx.NewConnection("windows-search"))
                {
                    cn.EnsureConnectionIsOpen();

                    Assert.IsTrue(cn.ImmediateExecuteEnumerable(
                        c =>
                        {
                            c.CommandText = spec.CmdText;
                            prepareInvoked = true;
                        })
                                    .Select(r => r.GetString(0))
                                    .Any());

                    Assert.IsTrue(prepareInvoked);
                }
            }
        }

        [Test]
        public void ImmediateExecuteEnumerable_ExecutesAndEnumerableUsesTransform()
        {
            var spec =
                new
                {
                    CmdText = "SELECT Top 25 System.ItemPathDisplay, System.ItemType FROM SYSTEMINDEX",
                    CmdType = CommandType.Text
                };

            using (var ctx = DbContext.SharedOrNewContext())
            {
                using (var cn = ctx.NewConnection("windows-search"))
                {
                    cn.EnsureConnectionIsOpen();

                    // assumes there are 25 or more indexed files: a pretty safe assumption.
                    Assert.AreEqual(25, cn.ImmediateExecuteEnumerable(spec.CmdText,
                        record => new
                        {
                            ItemPathDisplay = record.GetString(0),
                            ItemType = record.GetString(1)
                        })
                                          .Count());
                }
            }
        }

        [Test]
        public void ImmediateExecuteNonQuery_()
        {
            var ddl = @"
CREATE TABLE [dbo].[TestWidgets]
	(
        [ID] INT IDENTITY(1, 1) NOT NULL
			CONSTRAINT PK_TestWidgets PRIMARY KEY,
		[Name] NVARCHAR(24) NOT NULL,
		[Description] NVARCHAR(2000) NULL,
		[Active] BIT NOT NULL
	)
";
            var rand = new Random(Environment.TickCount);
            var gen = new DataGenerator();
            var originalData = new List<Tuple<int, string, string, bool>>();

            using (var ctx = DbContext.SharedOrNewContext())
            {
                using (var cn = ctx.NewConnection("adoWrapper"))
                {
                    cn.EnsureConnectionIsOpen();

                    cn.ImmediateExecuteNonQuery(ddl);

                    for (var i = 0; i < 100; ++i)
                    {
                        var item = Tuple.Create(
                            gen.GetString(rand.Next(12, 24)),
                            gen.GetStringWithLineBreaks(2000),
                            rand.Next() % 3 == 0);

                        Assert.AreEqual(1, cn.ImmediateExecuteNonQuery(@"
INSERT INTO [dbo].[TestWidgets] (
  [Name], 
  [Description], 
  [Active] )
VALUES (
  @Name,
  @Description,
  @Active
)
SELECT @ID = SCOPE_IDENTITY()",
                            binder =>
                            {
                                binder.DefineParameter("ID", DbType.Int32, ParameterDirection.Output);
                                binder.DefineAndBindParameter("Name", item.Item1);
                                binder.DefineAndBindParameter("Description", item.Item2);
                                binder.DefineAndBindParameter("Active", item.Item3);
                            },
                            (cmd, binder, res) => originalData.Add(Tuple.Create(
                                binder.GetInt32(cmd, "ID"),
                                item.Item1,
                                item.Item2,
                                item.Item3)
                                                      )));
                    }
                }
            }
        }
    }
}