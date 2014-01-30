using System;
using System.Text;
using FlitBit.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.Tests.Model
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
