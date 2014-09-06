using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Transactions;
using FlitBit.Core;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta.DDL;
using FlitBit.Data.SPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Data.Tests.Patterns
{
  [TestClass]
  public class DescriptionTest
  {
    [TestMethod]
    public void Description_CRUD()
    {
      var mapping = DataModel<IDescription>.Mapping;
      var binder = DataModel<IDescription>.Binder;
      var batch = mapping.GetDdlBatch(DDLBehaviors.Create);
      var repo = DataModel<IDescription>.GetRepository<int>();


      var all = repo.All(QueryBehavior.Default);
      foreach (var item in all.Results)
      {
        
      }

    }

    [TestMethod]
    public void FakeDescriptionTest()
    {
      var untypedRepo = DataModel<IDescription>.GetRepository<int>();

      // To create a custom query you need to upcast the repository with the appropriate DbConnection type
      // for the DB upon which the type is mapped...
      var repo = (IDataModelRepository<IDescription, int, SqlConnection>)untypedRepo;

      // Define a query by name... this creates an emitted, pre-compiled query definition
      // for getting Description by name.
      var byName = repo.QueryBuilder.Where<string>((model, name) => model.Name == name);

      var generator = new DataGenerator();
      var rand = new Random(Environment.TickCount);
      var fakeData = new List<Tuple<string, string, string>>();
      for (var i = 0; i < 20; i++)
      {
        var seed = rand.Next();
        // approx every 5th name will be null,
        // approx every 3rd short description will be null
        // approx every other long description will be null
        fakeData.Add(Tuple.Create(
          (seed % 5 == 0) ? null : generator.GetWords(rand.Next(4, 12)),
          (seed % 3 == 0) ? null : generator.GetWords(rand.Next(10, 80)),
          (seed % 2 == 0) ? null : generator.GetWords(rand.Next(30, 300))
          ));
      }

      Queue<int> cleanupIds = new Queue<int>();

      // Use a transaction so we don't leave test data in the db...
      using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew))
      using (var cx = DbContext.NewContext())
      {
        IDescription existing;
        foreach (var data in fakeData)
        {
          // Do a lookup by name before we insert...
          if (data.Item1 != null)
          {
            existing = repo.ExecuteSingle(byName, cx, data.Item1);

            // If the generated data matches data already present then skip it...
            if (existing != null) continue;
          }

          var model = FactoryProvider.Factory.CreateInstance<IDescription>();

          model.Language = 1;
          model.Name = data.Item1;
          model.ShortDescription = data.Item2;
          model.LongDescription = data.Item3;

          // prior to saving it doesn't have an ID...
          Assert.AreEqual(0, model.ID);

          var created = repo.Create(cx, model);

          Assert.IsNotNull(created);
          Assert.AreNotEqual(0, created.ID);

          cleanupIds.Enqueue(created.ID);

          Assert.AreEqual(data.Item1, created.Name);
          Assert.AreEqual(data.Item2, created.ShortDescription);
          Assert.AreEqual(data.Item3, created.LongDescription);

          if (data.Item1 != null)
          {
            // Now look it up by name again...
            existing = repo.ExecuteSingle(byName, cx, data.Item1);

            Assert.IsNotNull(created);
            Assert.AreEqual(created, existing);
          }
          else
          {
            var cloneable = created as ICloneable;
            Assert.IsNotNull(cloneable, "emitter implements ICloneable on the implementation");

            existing = (IDescription)cloneable.Clone();
          }

          if (created.ShortDescription != null)
          {
            existing.ShortDescription = null;

            Assert.IsTrue(((IDataModel)existing).IsDirty("ShortDescription"), "The clone was updated");
            Assert.IsFalse(((IDataModel)created).IsDirty("ShortDescription"), "The original was not updated");
          }

          if (created.LongDescription != null)
          {
            existing.LongDescription = null;

            Assert.IsTrue(((IDataModel)existing).IsDirty("LongDescription"), "The clone was updated");
            Assert.IsFalse(((IDataModel)created).IsDirty("LongDescription"), "The original was not updated");
          }

          // Ok, save the updated data model...
          existing = repo.Update(cx, existing);

          Assert.AreEqual(0, ((IDataModel)existing).GetDirtyFlags().TrueFlagCount, "Newly retreived object is never dirty.");

          // They're gone in the db...
          Assert.IsNull(existing.ShortDescription);
          Assert.IsNull(existing.LongDescription);

          // Ok, put them back...

          existing.ShortDescription = data.Item2;
          existing.LongDescription = data.Item3;

          existing = repo.Update(cx, existing);

          Assert.AreEqual(0, ((IDataModel)existing).GetDirtyFlags().TrueFlagCount, "Newly retreived object is never dirty.");

          Assert.AreEqual(data.Item2, created.ShortDescription);
          Assert.AreEqual(data.Item3, created.LongDescription);
        }

        // We're not completing the transaction because we don't want to leave data...
        // If you change this, remember to disable it again!
        tx.Complete();
      }
    }

  }
}
