using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FlitBit.Core;
using FlitBit.Data.Catalog;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Data.Tests.Catalog.Models;
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
			var mapping = Mappings.Instance.ForType<IMappedType>();

			Assert.IsNotNull(mapping);
			Assert.IsNotNull(mapping.Columns);

			var binder = (IDataModelBinder<IMappedType,int, SqlConnection>)mapping.GetBinder();
			var builder = new StringBuilder(2000);
			binder.BuildDdlBatch(builder);
			var sql = builder.ToString();
			Assert.IsNotNull(mapping.ConcreteType);
			Assert.IsNotNull(sql);

			var all = binder.GetAllCommand();
			var create = binder.GetCreateCommand();
			var update = binder.GetUpdateCommand();
			var readByType = binder
				.MakeQueryCommand<Type>()
				.Where((model, runtimeType) => model.RuntimeType == runtimeType);

			using (var cx = DbContext.NewContext())
			{
				using (var cn = cx.SharedOrNewConnection<SqlConnection>("test-data"))
				{
					var them = all.ExecuteMany(cx, cn, QueryBehavior.Default);
					Assert.IsNotNull(them);
					Assert.IsTrue(them.Succeeded);

					var existing = readByType.ExecuteMany(cx, cn, QueryBehavior.Default, typeof(IMappedType));
					var res = existing.Results.SingleOrDefault();
					if (res != null)
					{
						var ver = typeof(IMappedType).Assembly
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
						update.ExecuteSingle(cx, cn, res);
					}
					else
					{
						var model = FactoryProvider.Factory.CreateInstance<IMappedType>();
						model.Catalog = "unitest";
						model.LatestVersion = typeof(IMappedType).Assembly.GetName()
																										.Version.ToString(3);
						model.Schema = "OrmCatalog";
						model.MappedTable = "MappedType";
						model.RuntimeType = typeof(IMappedType);
						model.OriginalVersion = model.LatestVersion;
						model.ReadObjectName = "MappedType";
						model.Strategy = MappingStrategy.OneClassOneTable; 

						var created = create.ExecuteSingle(cx, cn, model);
						Assert.IsNotNull(created);
					}


				}
			}
		}
	}
}