using System;
using System.Data.SqlClient;
using System.Text;
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
			mapping.ConnectionName = "test-data";

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
			var update = new UpdateMappedTypeCommand();
			var readByType = new ReadMappedTypeByRuntimeTypeCommand();

			using (var cx = DbContext.NewContext())
			{
				using (var cn = cx.SharedOrNewConnection<SqlConnection>("test-data"))
				{
					var them = all.ExecuteMany(cx, cn, QueryBehavior.Default);
					Assert.IsNotNull(them);
					Assert.IsTrue(them.Succeeded);

					var existing = readByType.ExecuteSingle(cx, cn, typeof(IMappedType));
					if (existing != null)
					{
						var ver = typeof(IMappedType).Assembly
																				.GetName()
																				.Version;
						if (existing.LatestVersion == ver.ToString(3))
						{
							existing.LatestVersion = new Version(ver.Major, ver.Minor, ver.Build + 1).ToString();
						}
						else
						{
							existing.LatestVersion = ver.ToString(3);
						}
						update.ExecuteSingle(cx, cn, existing);
					}
					else 
					{
						var model = new IMappedTypeDataModel();
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