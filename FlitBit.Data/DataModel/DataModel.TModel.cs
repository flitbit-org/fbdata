#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

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
		  var idType = IdentityKey.KeyType;
		  if (idType == null)
		  {
        throw new InvalidOperationException(String.Concat("Data model doesn't define an identity key: ", typeof(TModel).GetReadableSimpleName(), ".")); 
		  }
		  if (typeof (TIdentityKey) != IdentityKey.KeyType)
			{
				throw new InvalidOperationException(String.Concat("Invalid identity key type: ", typeof(TIdentityKey).GetReadableSimpleName(), ". The defined identity key type is ", idType.GetReadableSimpleName(), "."));
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