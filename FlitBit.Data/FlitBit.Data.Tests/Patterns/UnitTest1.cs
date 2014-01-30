using System;
using System.Text;
using FlitBit.Core;
using FlitBit.Data.DataModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Data.Tests.Patterns
{
  [TestClass]
  public class NullableDateTimeTests
  {
    [TestMethod]
    public void TestMethod1()
    {
      var binder = DataModel<ITestNullableDateTime>.Binder;

      var builder = new StringBuilder(2000);
      binder.BuildDdlBatch(builder);
      var sql = builder.ToString();
      Assert.IsNotNull(sql);

      var repo = DataModel<ITestNullableDateTime>.GetRepository<int>();


      var it = FactoryProvider.Factory.CreateInstance<ITestNullableDateTime>();
      it.TouchedDate = DateTime.Now;
      using (var cx = DbContext.NewContext())
      {
        var created = repo.Create(cx, it);
        Assert.IsNotNull(created);


      }
    }
  }
}
