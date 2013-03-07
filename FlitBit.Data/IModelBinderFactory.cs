using System.Data.Common;
using FlitBit.Data.Meta;

namespace FlitBit.Data
{
	public interface IModelBinderFactory
	{
		IModelBinder<TModel, Id> GetModelBinder<TModel, Id>(IDataModelCatalog catalog, MappingStrategy strategy);
	}
}
