using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Data.Tests
{
    [TestClass]
    public class DbConnectionExtensionsTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CreateCommand_with_CommandText()
        {
            var spec = new {CmdText = "SELECT Top 25 System.ItemPathDisplay FROM SYSTEMINDEX"};

            using (IDbContext ctx = DbContext.SharedOrNewContext())
            using (DbConnection cn = ctx.NewConnection("windows-search"))
            {
                Assert.IsNotNull(cn,
                    "There should be a connection in the ConnectionStrings configuration section with the name 'windows-search'");

                cn.EnsureConnectionIsOpen();
                using (DbCommand cmd = cn.CreateCommand(spec.CmdText))
                {
                    Assert.AreEqual(spec.CmdText, cmd.CommandText);
                }
            }
        }

        [TestMethod]
        public void CreateCommand_with_CommandText_CommandType()
        {
            var spec =
                new {CmdText = "SELECT Top 25 System.ItemPathDisplay FROM SYSTEMINDEX", CmdType = CommandType.Text};

            using (IDbContext ctx = DbContext.SharedOrNewContext())
            using (DbConnection cn = ctx.NewConnection("windows-search"))
            {
                Assert.IsNotNull(cn,
                    "There should be a connection in the ConnectionStrings configuration section with the name 'windows-search'");

                cn.EnsureConnectionIsOpen();
                using (DbCommand cmd = cn.CreateCommand(spec.CmdText, spec.CmdType))
                {
                    Assert.AreEqual(spec.CmdText, cmd.CommandText);
                    Assert.AreEqual(spec.CmdType, cmd.CommandType);
                }
            }
        }

        [TestMethod]
        public void ImmediateExecuteAndTransform()
        {
            var spec =
                new
                {
                    CmdText = "SELECT Top 1 System.ItemPathDisplay FROM SYSTEMINDEX",
                    CmdType = CommandType.Text,
                    CmdTimeout = 1000
                };

            using (DbConnection cn = DbExtensions.CreateAndOpenConnection("windows-search"))
            using (DbCommand cmd = cn.CreateCommand(spec.CmdText, spec.CmdType, spec.CmdTimeout))
            {
                string result = cmd.ExecuteSingle(r => r.GetString(0));

                // TODO: Revise this test so it gets a predictable result (other than the default 0) upon success.
                Assert.IsFalse(String.IsNullOrEmpty(result));
            }
        }

        [TestMethod]
        public void ImmediateExecuteAndTransform_Transform()
        {
            using (DbConnection cn = DbExtensions.CreateAndOpenConnection("windows-search"))
            {
                IEnumerable<IndexItemDTO> items =
                    cn.ImmediateExecuteEnumerable(
                        "SELECT Top 25 System.ItemPathDisplay, System.ItemType FROM SYSTEMINDEX",
                        d =>
                        {
                            return new IndexItemDTO
                            {
                                ItemPathDisplay = d.GetString(0),
                                ItemType = d.GetString(1)
                            };
                        });
                foreach (IndexItemDTO item in items)
                {
                    Assert.IsFalse(String.IsNullOrEmpty(item.ItemPathDisplay));
                    Assert.IsFalse(String.IsNullOrEmpty(item.ItemType));
                }
            }
        }

        [TestMethod]
        public void ImmediateExecuteEnumerable()
        {
            using (DbConnection cn = DbExtensions.CreateConnection("windows-search"))
            {
                cn.EnsureConnectionIsOpen();

                IEnumerable<IDataRecord> data =
                    cn.ImmediateExecuteEnumerable(
                        "SELECT TOP 25 System.ItemPathDisplay, System.ItemType FROM SYSTEMINDEX WHERE System.ItemType = '.lnk'");
                var items = from reader in data
                    select new
                    {
                        ItemPathDisplay = reader.GetString(0),
                        ItemType = reader.GetString(1)
                    };

                foreach (var item in items)
                {
                    Assert.IsFalse(String.IsNullOrEmpty(item.ItemPathDisplay));
                    Assert.IsFalse(String.IsNullOrEmpty(item.ItemType));
                }
            }
        }

        private class IndexItemDTO
        {
            public string ItemPathDisplay { get; set; }
            public string ItemType { get; set; }
        }

        //[TestMethod]
        //public void AsyncExecuteAndTransform()
        //{
        //  using (var ctx = DbContext.SharedOrNewContext())
        //  {
        //    var cn = ctx.NewConnection("windows-search");
        //    cn.EnsureConnectionIsOpen();

        //    IFuture<IEnumerable<IndexItemDTO>> futr = cn.AsyncExecuteAndTransform<IndexItemDTO>(scope
        //      , @"SELECT Top 250 System.ItemPathDisplay, System.ItemType FROM SYSTEMINDEX WHERE System.ItemType = '.lnk'"
        //      , (d) =>
        //      {
        //        return new IndexItemDTO
        //        {
        //          ItemPathDisplay = d.GetString(0),
        //          ItemType = d.GetString(1)
        //        };
        //      }
        //      , null
        //      );

        //    Stopwatch clock = new Stopwatch();
        //    clock.Start();

        //    IEnumerable<IndexItemDTO> items = futr.Value;
        //    string elapsed = clock.Elapsed.ToString();
        //    foreach (var item in items)
        //    {
        //      Assert.IsFalse(String.IsNullOrEmpty(item.ItemPathDisplay));
        //      Assert.IsFalse(String.IsNullOrEmpty(item.ItemType));
        //    }
        //  }
        //}
    }
}