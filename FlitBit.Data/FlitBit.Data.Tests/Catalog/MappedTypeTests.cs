using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlitBit.Data.Meta;
using FlitBit.Data.Catalog;
using FlitBit.Emit;

namespace FlitBit.Data.Tests.Catalog
{
	[TestClass]
	public class MappedTypeTests
	{
		[TestInitialize]
		public void Initialize()
		{
			// force the dynamic assembly to disk so we can put eyeballs on the code...
			RuntimeAssemblies.WriteDynamicAssemblyOnExit = true;
		}

		[TestMethod]
		public void TestMethod1()
		{
			var mapping = Mappings.Instance.ForType<IMappedType>();
			mapping.ConnectionName = "test-data";

			Assert.IsNotNull(mapping);
			Assert.IsNotNull(mapping.Columns);

			var binder = mapping.GetBinder();
			var builder = new StringBuilder(2000);
			binder.BuildDdlBatch(builder);
			var sql = builder.ToString();
			Assert.IsNotNull(sql);
		}
	}
}
