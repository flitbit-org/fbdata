using FlitBit.Data.Meta;

namespace FlitBit.Data
{
	public interface IDataModelCatalog
	{
		void Register<TModel, Id>(Mapping<TModel> mapping, IModelBinder<TModel, Id> binder);
	}
}