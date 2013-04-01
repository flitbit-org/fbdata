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
			if (typeof(TIdentityKey) == Mapping<TModel>.Instance.IdentityKey.KeyType && Repository<TIdentityKey>.HasRepository(Mapping<TModel>.Instance))
			{}
			return default(TModel);
		}

		internal static class Repository<TIdentityKey>
		{
			public static bool HasRepository(Mapping<TModel> mapping)
			{
				return (!String.IsNullOrEmpty(mapping.ConnectionName))
					&& DbProviderHelpers.GetDbProviderHelperForDbConnection(mapping.ConnectionName) != null;
			}
		}
	}
}