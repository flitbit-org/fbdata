using FlitBit.Data.Meta;

namespace FlitBit.Data.DataModel
{
	public interface IDataModelCatalog
	{
		void Register<TDataModel, TIdentityKey>(IMapping<TDataModel> mapping, IDataModelBinder<TDataModel, TIdentityKey> binder);
	}
}