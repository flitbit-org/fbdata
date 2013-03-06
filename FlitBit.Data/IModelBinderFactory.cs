using System.Data.Common;

namespace FlitBit.Data
{
	public interface IModelBinderFactory
	{
		IModelBinder<TModel, Id, TModelImpl, TDbConnection> GetModelBinder<TModel, Id, TModelImpl, TDbConnection>(IDataModelCatalog catalog, ModelBindingScheme scheme)
			where TModelImpl : class, TModel, new()
			where TDbConnection : DbConnection;
	}
}
