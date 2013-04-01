using System.Linq;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Data.Tests.Meta.Models;
using FlitBit.Emit;
using FlitBit.Wireup;
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
			var party = Mapping<IParty>.Instance;
			var idk = party.IdentityKeyType;
			Assert.IsNotNull(party);
			Assert.IsNotNull(party.Columns);
			Assert.AreEqual(4, party.Columns.Count());

			var binder = party.GetBinder();

			var builder = new StringBuilder(2000);
			binder.BuildDdlBatch(builder);
			var sql = builder.ToString();

			Assert.IsNotNull(sql);

			var people = Mapping<IPerson>.Instance;
			idk = Mapping<IParty>.Instance.IdentityKeyType;

			Assert.IsNotNull(people);

			Assert.IsNotNull(people.Columns);

			// IParty: 4, IPerson: 4 , IEmailAddress: 2
			Assert.AreEqual(10, people.Columns.Count());

			var peepsBinder = people.GetBinder();

			builder = new StringBuilder(2000);
			peepsBinder.BuildDdlBatch(builder);
			sql = builder.ToString();

			Assert.IsNotNull(sql);

			var organizations = Mapping<IOrganization>.Instance;
			var groups = Mapping<IGroup>.Instance;

			var parties = party.Hierarchy.KnownSubtypes;
			Assert.IsNotNull(parties);
			Assert.IsNotNull(party.ConcreteType);
			Assert.IsNotNull(people.ConcreteType);
		}
	}
}