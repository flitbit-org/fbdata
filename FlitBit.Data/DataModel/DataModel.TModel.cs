using System;
using FlitBit.Data.Meta;

namespace FlitBit.Data.DataModel
{
	/// <summary>
	/// Data model utilities.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	public static class DataModel<TModel>
	{
		internal static TModel ResolveIdentityKey<TIdentityKey>(TIdentityKey id)
		{
			var mapping = Mapping<TModel>.Instance;
			if (typeof (TIdentityKey) != mapping.IdentityKey.KeyType || !mapping.HasBinder) return default(TModel);
			var binder = (IDataModelBinder<TModel, TIdentityKey>)mapping.GetBinder();
			var repo = binder.MakeRepository();
			return repo.ReadByIdentity(DbContext.Current, id);
		}
	}
}