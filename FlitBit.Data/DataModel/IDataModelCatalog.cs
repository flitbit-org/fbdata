using FlitBit.Data.Meta;

namespace FlitBit.Data.DataModel
{
	public interface IDataModelCatalog
	{
		void Register<TModel, Id>(Mapping<TModel> mapping, IDataModelBinder<TModel, Id> binder);
	}
}