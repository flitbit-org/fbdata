using System;
using FlitBit.Data.Meta;
using FlitBit.Data.Tests.TestModel0;
using FlitBit.ObjectIdentity;
using NUnit.Framework;

namespace FlitBit.Data.Tests
{
    namespace TestModel0
    {
        [MapEntity(EntityBehaviors.MapAllProperties | EntityBehaviors.Pluralized, MappingStrategy.OneClassOneTable,
            ConnectionName = "adoWrapper", TargetSchema = "TestModel0")]
        public interface IPerson
        {
            [IdentityKey]
            [MapColumn(ColumnBehaviors.Synthetic)]
            int PersonID { get; }

            [MapColumn(ColumnBehaviors.Nullable, 40)]
            string FirstName { get; set; }

            [MapColumn(ColumnBehaviors.Nullable, 40)]
            string LastName { get; set; }

            [MapColumn(ColumnBehaviors.Nullable, 40)]
            string MiddleNames { get; set; }

            [MapColumn(ColumnBehaviors.TimestampOnInsert)]
            DateTime DateCreated { get; }
        }
    }

    [TestFixture]
    public class DataModelRepositoryTests : DataModelTests<IPerson, int>
    {
        [SetUp]
        public void Setup()
        {
            CreateStorage();
        }

        [Test]
        public void Monkey()
        {}
    }
}