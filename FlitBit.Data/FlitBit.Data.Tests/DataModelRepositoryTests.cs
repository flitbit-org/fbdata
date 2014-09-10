using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FlitBit.Core;
using FlitBit.Data.DataModel;
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
        IDataModelRepository<IPerson, int, SqlConnection> _repository;
        IList<IPerson> _items;

        [TestFixtureSetUp]
        public void Setup()
        {
            _repository = CreateStorage<SqlConnection>();
            _items = new List<IPerson>();
            using (var cx = DbContext.NewContext())
            {
                foreach (var item in this.CreateDataModels(100))
                {
                    _items.Add(_repository.Create(cx, item));
                }
            }
        }

        [Test]
        public void DataModelRepository_All_WithUnlimitedBehavior_RetrievesAllRows()
        {
            using (var cx = DbContext.NewContext())
            {
                var all = _repository.All(cx);
                
                Assert.AreEqual(_items.Count, all.Results.Count());
            }
        }

        [Test]
        public void DataModelRepository_All_WithLimitedBehavior_RetrievesLimitedRows()
        {
            using (var cx = DbContext.NewContext())
            {
                var behaviors = new QueryBehavior(QueryBehaviors.Paged, 10, 1, 0);
                var all = _repository.All(cx, behaviors);

                Assert.AreEqual(behaviors.Limit, all.Results.Count());
            }
        }

        [Test]
        public void DataModelRepository_Read()
        {
            var random = new Random();
            for (int i = 0; i < _items.Count; i++)
            {
                var randomItem = _items[random.Next(_items.Count - 1)];
                using (var cx = DbContext.NewContext())
                {
                    var retreived = _repository.ReadByIdentity(cx, randomItem.PersonID);
                    Assert.AreEqual(randomItem, retreived);
                }
            }
        }

        protected override void PopulateItem(DataGenerator gen, IPerson item)
        {
            var ran = new Random();
            item.FirstName = gen.GetString(ran.Next(20, 40));
            item.LastName = gen.GetString(ran.Next(20, 40));
        }
    }
}