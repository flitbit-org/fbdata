using System.Linq;
using System.Text;
using FlitBit.Data.Tests.Meta.Models;
using FlitBit.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Data.Tests.Meta
{
	[TestClass]
	public class HierarchyMappingTests
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
			var party = DataModel<IParty>.Mapping;
			var idk = DataModel<IParty>.IdentityKey;
			Assert.IsNotNull(party);
			Assert.IsNotNull(party.Columns);
			Assert.AreEqual(5, party.Columns.Count());

			var binder = party.GetBinder();

			var builder = new StringBuilder(2000);
			binder.BuildDdlBatch(builder);
			var sql = builder.ToString();

			Assert.IsNotNull(sql);

			var people = DataModel<IPerson>.Mapping;
			idk = DataModel<IParty>.IdentityKey;

			Assert.IsNotNull(people);

			Assert.IsNotNull(people.Columns);

			// IParty: 5, IPerson: 4, IEmailAddress: 2
			Assert.AreEqual(11, people.Columns.Count());

			var peepsBinder = people.GetBinder();

			builder = new StringBuilder(2000);
			peepsBinder.BuildDdlBatch(builder);
			sql = builder.ToString();

			Assert.IsNotNull(sql);

			var organizations = DataModel<IOrganization>.Mapping;
			var groups = DataModel<IGroup>.Mapping;

			var parties = DataModel<IParty>.Hierarchy.KnownSubtypes;
			Assert.IsNotNull(parties);
		}
	}
}