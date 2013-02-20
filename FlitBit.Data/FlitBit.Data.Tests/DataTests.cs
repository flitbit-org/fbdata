using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlitBit.Data;

namespace FlitBit.Core.Tests.Data
{
	[TestClass]
	public class DataTests
	{
		[TestInitialize]
		public void Init()
		{
		}

		[TestMethod]
		public void Monkey()
		{
			using(var context = DbContext.SharedOrNewContext())
			using (var cn = context.NewOrSharedConnection("test-data"))
			using (var cmd = cn.CreateCommand())
			{
				// TODO
			}
		}
	}
}
