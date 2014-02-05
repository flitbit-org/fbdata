using System;
using System.Text;
using FlitBit.Core;
using FlitBit.Data.DataModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Data.Tests.Patterns
{
  [TestClass]
  public class NullableDecimalTests
  {
    [TestMethod]
    public void SqlClientMapping_CanReadAndWriteNullableDecimal()
    {
      var binder = DataModel<ITestNullableDecimal>.Binder;

      var builder = new StringBuilder(2000);
      binder.BuildDdlBatch(builder);
      var sql = builder.ToString();
      Assert.IsNotNull(sql);

      var repo = DataModel<ITestNullableDecimal>.GetRepository<int>();
      var gen = new DataGenerator();

      for (var i = 0; i < 100; i++)
      {
        var it = FactoryProvider.Factory.CreateInstance<ITestNullableDecimal>();
        // Random decimal of possible full money precision/scale
          var whole = gen.GetInt32() % 999999999999999;
          var frac = Math.Abs(gen.GetInt32() % 9999);
          decimal val;
        // bad format will result in some nulls...
        if (decimal.TryParse(String.Format("{0}.{1}", whole, frac), out val))
          {
            it.Value = val;
          }
        it.DateCreated = DateTime.UtcNow;

        ITestNullableDecimal created;
        using (var cx = DbContext.NewContext())
        {
          created = repo.Create(cx, it);
          Assert.IsNotNull(created);
        }
        // Use seperate context to ensure we don't just grab the created from mem...
        using (var cx = DbContext.NewContext())
        {
          var read = repo.ReadByIdentity(cx, created.ID);
          Assert.IsNotNull(read);
          Assert.AreEqual(created.ID, read.ID);
          Assert.AreEqual(created.Value, read.Value);
        }
      }
    }
  }
}