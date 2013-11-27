using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FlitBit.Core;
using FlitBit.Data.Catalog;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Emit;
using FlitBit.Wireup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Data.Tests.Catalog
{
	[TestClass]
	public class MappedTypeTests
	{
		[TestInitialize]
		public void Initialize()
		{
			WireupCoordinator.SelfConfigure();
			// force the dynamic assembly to disk so we can put eyeballs on the code...
			RuntimeAssemblies.WriteDynamicAssemblyOnExit = true;
		}

		[TestMethod]
		public void OneTypeOneTableMappingTest()
		{
			var untypedRepo = DataModel<IMappedType>.GetRepository<int>();
			var repo = (IDataModelRepository<IMappedType, int, SqlConnection>) untypedRepo;

			var binder = DataModel<IMappedType>.Binder;
			var builder = new StringBuilder(2000);
			binder.BuildDdlBatch(builder);
			var sql = builder.ToString();
			Assert.IsNotNull(sql);

			var byRuntimeType = repo.QueryBuilder.Where<Type>((model, runtimeType) => model.RuntimeType == runtimeType);

			using (var cx = DbContext.NewContext())
			{
				using (var cn = cx.SharedOrNewConnection<SqlConnection>("test-data"))
				{
					var them = repo.All(cx);
					Assert.IsNotNull(them);
					Assert.IsTrue(them.Succeeded);

					var existing = repo.ExecuteMany(byRuntimeType, cx, QueryBehavior.Default, typeof (IMappedType));
					var res = existing.Results.SingleOrDefault();
					if (res != null)
					{
						var ver = typeof (IMappedType).Assembly
							.GetName()
							.Version;
						if (res.LatestVersion == ver.ToString(3))
						{
							res.LatestVersion = new Version(ver.Major, ver.Minor, ver.Build + 1).ToString();
						}
						else
						{
							res.LatestVersion = ver.ToString(3);
						}
						repo.Update(cx, res);
					}
					else
					{
						var model = FactoryProvider.Factory.CreateInstance<IMappedType>();
						model.Catalog = "unitest";
						model.LatestVersion = typeof (IMappedType).Assembly.GetName()
							.Version.ToString(3);
						model.Schema = "OrmCatalog";
						model.MappedTable = "MappedType";
						model.RuntimeType = typeof (IMappedType);
						model.OriginalVersion = model.LatestVersion;
						model.ReadObjectName = "MappedType";
						model.Strategy = MappingStrategy.OneClassOneTable;

						var created = repo.Create(cx, model);
						Assert.IsNotNull(created);
					}
				}
			}
		}
	}
}