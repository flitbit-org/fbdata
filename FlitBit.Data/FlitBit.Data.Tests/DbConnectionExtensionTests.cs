using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Common;
using System.Data;
using System.Diagnostics;
using FlitBit.Core.Parallel;

namespace FlitBit.Data.Tests
{
	[TestClass]
	public class DbConnectionExtensionsTests
	{
		public DbConnectionExtensionsTests() { }
		public TestContext TestContext { get; set; }

		[TestMethod]
		public void CreateCommand_with_CommandText_CommandType()
		{
			var spec = new { CmdText = "SELECT Top 25 System.ItemPathDisplay FROM SYSTEMINDEX", CmdType = CommandType.Text };

			using (var ctx = DbContext.SharedOrNewContext())
			using (var cn = ctx.NewConnection("windows-search"))
			{
				Assert.IsNotNull(cn, "There should be a connection in the ConnectionStrings configuration section with the name 'windows-search'");

				cn.EnsureConnectionIsOpen();
				using (var cmd = cn.CreateCommand(spec.CmdText, spec.CmdType))
				{																										
					Assert.AreEqual(spec.CmdText, cmd.CommandText);
					Assert.AreEqual(spec.CmdType, cmd.CommandType);

				}
			}
		}
		[TestMethod]
		public void CreateCommand_with_CommandText()
		{
			var spec = new { CmdText = "SELECT Top 25 System.ItemPathDisplay FROM SYSTEMINDEX" };

			using (var ctx = DbContext.SharedOrNewContext())
			using (var cn = ctx.NewConnection("windows-search"))
			{
				Assert.IsNotNull(cn, "There should be a connection in the ConnectionStrings configuration section with the name 'windows-search'");

				cn.EnsureConnectionIsOpen();
				using (var cmd = cn.CreateCommand(spec.CmdText))
				{

					Assert.AreEqual(spec.CmdText, cmd.CommandText);
				}
			}
		}
				

		[TestMethod]
		public void ImmediateExecuteAndTransform()
		{
			var spec = new { CmdText = "SELECT Top 1 System.ItemPathDisplay FROM SYSTEMINDEX", CmdType = CommandType.Text, CmdTimeout = 1000 };

			using (var ctx = DbContext.SharedOrNewContext())
			using (var cn = ctx.NewConnection("windows-search"))
			{
				Assert.IsNotNull(cn, "There should be a connection in the ConnectionStrings configuration section with the name 'windows-search'");

				cn.EnsureConnectionIsOpen();
				string result = cn.ExecuteReader(spec.CmdText, spec.CmdType, spec.CmdTimeout).TransformSingle(r => r.GetString(0));

				// TODO: Revise this test so it gets a predictable result (other than the default 0) upon success.
				Assert.IsFalse(String.IsNullOrEmpty(result));
			}
		}
				
		class IndexItemDTO
		{
			public string ItemPathDisplay { get; set; }
			public string ItemType { get; set; }
		}

		[TestMethod]
		public void ImmediateExecuteAndTransform_Transform()
		{
			using (var ctx = DbContext.SharedOrNewContext())
			{
				var cn = ctx.NewConnection("windows-search");
				Assert.IsNotNull(cn, "There should be a connection in the ConnectionStrings configuration section with the name 'windows-search'");

				cn.EnsureConnectionIsOpen();
			
				var items = cn.ExecuteReader("SELECT Top 25 System.ItemPathDisplay, System.ItemType FROM SYSTEMINDEX")
					.TransformAll((d) =>
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
			using (var ctx = DbContext.SharedOrNewContext())
			{
				var cn = ctx.NewConnection("windows-search");
				cn.EnsureConnectionIsOpen();
			
				var items = from reader in cn.ExecuteReader("SELECT TOP 25 System.ItemPathDisplay, System.ItemType FROM SYSTEMINDEX")
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
