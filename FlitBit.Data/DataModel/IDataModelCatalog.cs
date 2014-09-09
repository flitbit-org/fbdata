#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Data.DataModel
{
    public interface IDataModelCatalog
    {
        void Register<TDataModel, TIdentityKey>(IMapping<TDataModel> mapping,
            IDataModelBinder<TDataModel, TIdentityKey> binder);
    }
}