using System;
using FlitBit.Core;
using FlitBit.Data.Meta;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data.DataModel
{
	/// <summary>
	/// Data model utilities.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	public static class DataModel<TModel>
	{
		/// <summary>
		/// The data model's identity key.
		/// </summary>
		public static IdentityKey<TModel> IdentityKey = FactoryProvider.Factory.CreateInstance<IdentityKey<TModel>>();

		/// <summary>
		/// The data model's mapping.
		/// </summary>
		public static IMapping<TModel> Mapping = Mappings.Instance.ForType<TModel>();

		/// <summary>
		/// The data model's concrete type.
		/// </summary>
		public static Type ConcreteType {
			get
			{
				if (typeof (TModel).IsInterface)
				{
					return DataModelEmitter.ConcreteType(Mapping);
				}
				return typeof (TModel);
			} 
		}

		public static IDataModelBinder Binder
		{
			get { return Mapping.GetBinder(); }
		}

		public static IDataModelRepository<TModel, TIdentityKey> GetRepository<TIdentityKey>()
		{
			if (typeof (TIdentityKey) != IdentityKey.KeyType)
			{
				throw new InvalidOperationException(String.Concat("Invalid identity key type: ", typeof(TIdentityKey).GetReadableSimpleName(), ". The defined identity key type is ", IdentityKey.KeyType.GetReadableSimpleName(), "."));
			}
			var binder = (IDataModelBinder<TModel, TIdentityKey>)Binder;
			return binder.MakeRepository();
		}

		internal static TModel ResolveIdentityKey<TIdentityKey>(TIdentityKey id)
		{
			var repo = GetRepository<TIdentityKey>();
			return repo.ReadByIdentity(DbContext.Current, id);
		}
	}
}