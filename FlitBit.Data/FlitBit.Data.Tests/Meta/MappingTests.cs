using System.Linq;
using FlitBit.Data.Meta.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Data.Meta.Tests
{
	[TestClass]
	public class MappingTests
	{
		[TestMethod]
		public void Monkey()
		{
			Mapping.Instance
				//.UseDefaultConnection("test-data")
				.ForType<IPerson>()
					.Column(x => x.ID).WithBehaviors(ColumnBehaviors.Synthetic).End()
					.Column(x => x.ExternalID).WithBehaviors(ColumnBehaviors.AlternateKey).End()
					.Column(x => x.Name).WithVariableLength(50).End()
					.Collection(p => p.PhoneNumbers).JoinReference<IPhone>(p => p.Person).End()
					.End();

			// Check the mapping for People...
			var people = Mapping.Instance.ForType<IPerson>();

			Assert.IsTrue(people.ParticipatingMembers.Contains(typeof(IPerson).GetProperty("ID")));
			Assert.IsTrue(people.ParticipatingMembers.Contains(typeof(IPerson).GetProperty("ExternalID")));
			Assert.IsTrue(people.ParticipatingMembers.Contains(typeof(IPerson).GetProperty("Name")));

			// It knows the runtime type...
			Assert.AreEqual(typeof(IPerson), people.RuntimeType);

			// We mapped two columns...
			Assert.AreEqual(3, people.Columns.Count());

			// We established ID as the identity column...
			Assert.AreEqual(1, people.Identity.Columns.Count());
			var identity = people.Identity.Columns.First();
			Assert.AreEqual("ID", identity.TargetName);

			// Since no natural key was specified, identity is the preferred reference column...
			Assert.AreEqual(identity, people.GetPreferredReferenceColumn());

			var columns = people.Columns;

			var id = Enumerable.First(from c in columns
																where c.TargetName == "ID"
																select c);

			Assert.AreEqual("Person.ID", id.DbObjectReference);
			// things we specified...		
			Assert.IsTrue(id.IsIdentity);
			Assert.IsTrue(id.IsImmutable);
			Assert.IsTrue(id.IsSynthetic);
			// things inferred
			Assert.IsTrue(id.IsCalculated); // because of 'IsSynthetic'
			Assert.IsFalse(id.IsNullable);
			// things we didn't specify
			Assert.IsFalse(id.IsAlternateKey);
			Assert.IsFalse(id.IsReference);
			Assert.IsFalse(id.IsRevisionTracking);
			Assert.IsFalse(id.IsTimestampOnInsert);
			Assert.IsFalse(id.IsTimestampOnUpdate);
			Assert.AreEqual(0, id.VariableLength);

			var ext = Enumerable.First(from c in columns
																where c.TargetName == "ExternalID"
																select c);

			Assert.AreEqual("Person.ExternalID", ext.DbObjectReference);
			// things we specified...		
			Assert.IsTrue(ext.IsAlternateKey);
			// things we didn't specify
			Assert.IsFalse(ext.IsCalculated);
			Assert.IsFalse(ext.IsIdentity);
			Assert.IsFalse(ext.IsImmutable);
			Assert.IsFalse(ext.IsNullable);
			Assert.IsFalse(ext.IsSynthetic);
			Assert.IsFalse(ext.IsReference);
			Assert.IsFalse(ext.IsRevisionTracking);
			Assert.IsFalse(ext.IsTimestampOnInsert);
			Assert.IsFalse(ext.IsTimestampOnUpdate);
			Assert.AreEqual(0, ext.VariableLength);

			var nm = Enumerable.First(from c in columns
																 where c.TargetName == "Name"
																 select c);

			Assert.AreEqual("Person.Name", nm.DbObjectReference);
			// things we specified...		
			Assert.AreEqual(50, nm.VariableLength);
			// things we didn't specify
			Assert.IsFalse(nm.IsAlternateKey);
			Assert.IsFalse(nm.IsCalculated);
			Assert.IsFalse(nm.IsIdentity);
			Assert.IsFalse(nm.IsImmutable);
			Assert.IsFalse(nm.IsNullable);
			Assert.IsFalse(nm.IsSynthetic);
			Assert.IsFalse(nm.IsReference);
			Assert.IsFalse(nm.IsRevisionTracking);
			Assert.IsFalse(nm.IsTimestampOnInsert);
			Assert.IsFalse(nm.IsTimestampOnUpdate);

			Assert.AreEqual(1, people.Collections.Count());

			// Verify the collection type...
			var phones = people.Collections.First();
			Assert.IsNotNull(phones.ReferenceJoinMember);
			Assert.AreEqual("Person", phones.ReferenceJoinMember.Name);

			people.ConnectionName = "test-data";
			people.TargetCatalog = "testing";
			var ddl = people.GetDdlBatch(DDL.DDLBehaviors.Create);
			Assert.IsNotNull(ddl);
			Assert.IsNotNull(ddl.Name);
		}
	}
}
