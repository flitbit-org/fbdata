using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Transactions;
using FlitBit.Core;
using FlitBit.Data.DataModel;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;
using NUnit.Framework;

namespace FlitBit.Data.Tests
{
    public abstract class DataModelTests<TDataModel, TIdentityKey>
    {
        public DataModelTests()
            : this(10, 10) { }

        public DataModelTests(int pageSize, int pageCount)
        {
            PageSize = pageSize;
            PageCount = pageCount;
        }

        public IDataModelRepository<TDataModel, TIdentityKey, TDbConnection> CreateStorage<TDbConnection>()
            where TDbConnection: DbConnection
        {
            var binder = DataModel<TDataModel>.Binder;
            var builder = new SqlWriter(2000, Environment.NewLine, "\t");
            binder.BuildDdlBatch(builder);
            var sql = builder.Text;
            using (var tx = new TransactionScope(TransactionScopeOption.Required))
            {
                using (var cn = ConnectionProviders.GetDbConnection(binder.UntypedMapping.ConnectionName))
                {
                    cn.Open();
                    cn.ImmediateExecuteNonQuery(sql);
                    tx.Complete();
                }
            }
            return DataModel<TDataModel>.GetRepository<TIdentityKey>() as
                IDataModelRepository<TDataModel, TIdentityKey, TDbConnection>;
        }

        public IList<TDataModel> CreateDataModels(int count)
        {
            var gen = new DataGenerator();
            var res = new List<TDataModel>();
            var factory = FactoryProvider.Factory;
            for (var i = 0; i < count; i++)
            {
                var item = factory.CreateInstance<TDataModel>();
                PopulateItem(gen, item);
                res.Add(item);
            }
            return res;
        }

        protected abstract void PopulateItem(DataGenerator gen, TDataModel item);

        public int PageSize { get; set; }
        public int PageCount { get; set; }

        public void BasicDataModelIOTest(Action<TDataModel> ea, Action<IDbContext, int> result)
        {
            var mapping = DataModel<TDataModel>.Mapping;
            if (mapping.Behaviors.HasFlag(EntityBehaviors.LookupList))
            {
                TestLookupListType(ea, result);
                return;
            }

            Console.WriteLine(String.Concat("Testing ", typeof(TDataModel).GetReadableSimpleName(),
                " as a data table. Processing ", PageCount, " pages of ", PageSize, " items."));

            var repo = DataModel<TDataModel>.GetRepository<TIdentityKey>();
            var paging = new QueryBehavior(QueryBehaviors.Paged, PageSize);
            var rand = new Random(Environment.TickCount);

            var count = 0;
            var pages = -1;

            using (var ctx = DbContext.NewContext())
            {
                while (++pages < PageCount)
                {
                    var them = repo.All(ctx, paging);
                    Assert.IsNotNull(them);
                    Assert.IsTrue(them.Succeeded);

                    foreach (var item in them.Results)
                    {
                        count++;
                        ea(item);
                    }

                    if (!them.Behaviors.HasNext)
                    {
                        break;
                    }

                    // next random page of data from available pages... (pages are 1 based)
                    var nextPage = rand.Next(((int)them.Behaviors.TotalCount / them.Behaviors.PageSize)) + 1;
                    paging = new QueryBehavior(them.Behaviors.Behaviors, them.Behaviors.PageSize, nextPage,
                        them.Behaviors.TotalCount);
                }
                result(ctx, count);
            }
        }

        void TestLookupListType(Action<TDataModel> ea,
            Action<IDbContext, int> result)
        {
            Console.WriteLine(String.Concat("Testing ", typeof(TDataModel).GetReadableSimpleName(), " as a lookup list."));

            var repo = DataModel<TDataModel>.GetRepository<TIdentityKey>();
            var count = 0;

            using (var ctx = DbContext.NewContext())
            {
                var them = repo.All(ctx);
                Assert.IsNotNull(them);
                Assert.IsTrue(them.Succeeded);

                foreach (var item in them.Results)
                {
                    count++;
                    ea(item);
                }

                result(ctx, count);
            }
        }
    }
}