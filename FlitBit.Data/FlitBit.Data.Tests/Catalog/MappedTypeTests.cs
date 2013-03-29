using System.Text;
using FlitBit.Data.Catalog;
using FlitBit.Data.Meta;
using FlitBit.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
			Assert.IsNotNull(mapping.ConcreteType);
			Assert.IsNotNull(sql);
		}
	}
}